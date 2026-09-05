using NoMoreFishAndChips.Effects;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Hitboxes;
using NoMoreFishAndChips.States;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

using Random = UnityEngine.Random;

namespace NoMoreFishAndChips.Entities
{
    public enum EFlyingFishState
    {
        None,
        Surface,
        Fly
    }

    public abstract class FlyingFishState : State<EFlyingFishState, ENone>
    {
        protected FlyingFish _fish;
        protected GameplayContext _context;

        public FlyingFishState(StateMachine<EFlyingFishState> parent, FlyingFish fish) : base(parent)
        {
            _fish = fish;
        }

        public void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }
    }

    public class FlyingFishSurfaceState : FlyingFishState
    {
        private FlyingFishSurfaceSettings _settings;

        private RaftEdge _edge;
        private Vector3 _targetPosition;

        public FlyingFishSurfaceState(StateMachine<EFlyingFishState> parent, FlyingFish fish) : base(parent, fish)
        {
            _settings = _fish.DefinitionData.SurfaceSettings;
        }

        public override void Enter()
        {
            base.Enter();

            Surface();
            ChooseDirection();
            WiggleThenFly();
        }

        // Surface away from the edge
        private void Surface()
        {
            _edge = Random.value <= 0.5f ? _fish.SpawnInfo.RaftLine.MinEdge : _fish.SpawnInfo.RaftLine.MaxEdge;

            Vector2Int cell = _edge.Node.Cell;
            cell += Utils.Math.DirectionToVector2Int(_edge.Direction) * _settings.OffsetRange.RandomRange();

            Vector3 position = _context.Raft.Queries.CellToWorldPosition(cell);
            position += new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
            position += Vector3.down * _settings.Depth;

            _fish.EntityPhysicsLogic.Rigidbody.position = position;

            EffectManager.SpawnVfxRpc(VfxId.WaterSplash, new Vector3(position.x, 0f, position.z));
        }

        // Choose a random point on either the edge or the tile behind it
        private void ChooseDirection()
        {
            List<RaftTile> tiles = ListPool<RaftTile>.Get();

            tiles.Add(_context.Raft.Tiles[_edge.Node.Cell]);

            if (_context.Raft.Tiles.TryGetValue(_edge.Node.Cell - Utils.Math.DirectionToVector2Int(_edge.Direction), out RaftTile tile))
            {
                tiles.Add(tile);
            }

            _targetPosition = tiles.OrderBy(tile => tile.EntityDefeatLogic.IsDefeated).ThenBy(tile => Random.value).First().transform.position;
            _targetPosition += new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
            _targetPosition.y = 0f;

            ListPool<RaftTile>.Release(tiles);
        }

        private void WiggleThenFly()
        {
            Quaternion getRotationToTargetPosition()
            {
                Vector3 direction = (_targetPosition - _fish.EntityPhysicsLogic.Rigidbody.position).normalized;
                direction.y = 0f;
                direction.Normalize();

                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

                return rotation;
            }

            Sequence.Create(updateType: UpdateType.FixedUpdate)
                // Wiggle while facing the target
                .Chain(Tween.Custom(startValue: 0f, endValue: 1f, duration: _settings.WiggleDuration, onValueChange: _ =>
                {
                    Quaternion rotation = getRotationToTargetPosition();
                    rotation *= Quaternion.AngleAxis(_settings.WigglePitch, Vector3.right);

                    _fish.EntityPhysicsLogic.Rigidbody.MoveRotation(rotation);
                }))
                // Pitch up
                .Chain(Tween.Custom(startValue: 0f, endValue: 1f, duration: _settings.PitchDuration, onValueChange: (float value) =>
                {
                    Quaternion startRotation = getRotationToTargetPosition();
                    startRotation *= Quaternion.AngleAxis(_settings.WigglePitch, Vector3.right);

                    float distance = Vector3.Distance(_fish.EntityPhysicsLogic.Rigidbody.position, _targetPosition);
                    float t = Mathf.InverseLerp(_settings.DistanceRange.Min, _settings.DistanceRange.Max, distance);
                    float angle = Mathf.Lerp(_settings.PitchRange.Min, _settings.PitchRange.Max, t);

                    Quaternion endRotation = getRotationToTargetPosition();
                    endRotation *= Quaternion.AngleAxis(angle, Vector3.right);

                    Quaternion rotation = Quaternion.Slerp(startRotation, endRotation, value);

                    _fish.EntityPhysicsLogic.Rigidbody.MoveRotation(rotation);
                }))
                .ChainCallback(() => _parentStateMachine.ChangeState(EFlyingFishState.Fly));
        }
    }

    public class FlyingFishFlyState : FlyingFishState
    {
        private HitboxManager _hitboxManager;
        private EntityManager _entityManager;

        private FlyingFishFlySettings _settings;

        private RaycastHit[] _markerHitsNonAlloc = new RaycastHit[4];

        private NetMarkerHandle _netMarkerHandle;

        private bool _readyToExplode;

        private Vector3? _blendPosition;

        public FlyingFishFlyState(StateMachine<EFlyingFishState> parent, FlyingFish fish) : base(parent, fish)
        {
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();

            _settings = _fish.DefinitionData.FlySettings;
        }

        public override void Enter()
        {
            base.Enter();

            _fish.EntityModel.Animator.SetBool(FlyingFish.IsFlyingBoolName, true);

            _fish.EntityPhysicsLogic.Rigidbody.AddForce(_fish.EntityPhysicsLogic.Rigidbody.rotation * Vector3.forward * _settings.ForceStrength, ForceMode.Impulse);

            Quaternion rotation = Quaternion.FromToRotation(_fish.EntityPhysicsLogic.Rigidbody.rotation * Vector3.forward, Vector3.down) * _fish.EntityPhysicsLogic.Rigidbody.rotation;

            PrimeTweenFix.RigidbodyMoveRotation(_fish.EntityPhysicsLogic.Rigidbody, endValue: rotation, duration: _settings.RotateDuration, ease: Ease.InOutQuad);

            _readyToExplode = false;

            _ = CreateMarkerAsync();
        }

        private async Task CreateMarkerAsync()
        {
            while (_fish.CharacterPhysicsModule.InWater)
            {
                await Task.Yield();
            }

            await Utils.Tasks.WaitForFixedUpdateAsync();

            _netMarkerHandle = _context.EnvironmentMarker.CreateNetMarker(CalculateMarkerPosition(), _settings.MarkerScale, 0f);
        }

        public override void Exit()
        {
            base.Exit();

            _netMarkerHandle.Remove();
            _netMarkerHandle = null;
        }
        
        public override void Tick()
        {
            base.Tick();

            ExplodeTick();
            MarkerTick();
        }
        
        private void ExplodeTick()
        {
            // Once airborne, we are ready to explode
            if (_fish.CharacterPhysicsModule.InAir)
            {
                _readyToExplode = true;
            }

            if (!_readyToExplode || _fish.CharacterPhysicsModule.InAir)
            {
                return;
            }

            _hitboxManager.SpawnHitbox(_settings.HitboxData, _fish, new SpawnParams() { Position = _fish.transform.position });
            _entityManager.Despawn(_fish);
        }

        private void MarkerTick()
        {
            if (_netMarkerHandle == null)
            {
                return;
            }

            // Position
            Vector3 position = CalculateMarkerPosition();

            _netMarkerHandle.SetPosition(position);

            // Blend
            float blend = 0f;

            if (_fish.EntityPhysicsLogic.Rigidbody.linearVelocity.y >= 0f)
            {
                _blendPosition = null;
            }
            else
            {
                if (!_blendPosition.HasValue)
                {
                    _blendPosition = _fish.EntityPhysicsLogic.Rigidbody.position;
                }

                float totalDistance = Vector3.Distance(_blendPosition.Value, position);
                float currentDistance = Vector3.Distance(_fish.EntityPhysicsLogic.Rigidbody.position, position);

                blend = _settings.BlendDistance < totalDistance ? Mathf.InverseLerp(totalDistance, _settings.BlendDistance, currentDistance) : 1f;
            }

            _netMarkerHandle.SetBlend(blend);
        }
        
        private Vector3 CalculateMarkerPosition()
        {
            Vector3 position = _fish.EntityPhysicsLogic.Rigidbody.position;
            Vector3 velocity = _fish.EntityPhysicsLogic.Rigidbody.linearVelocity;

            float time = 0f;

            while (time < _settings.CalculateDuration)
            {
                velocity += Physics.gravity * Time.fixedDeltaTime;
                velocity *= 1f / (1f + _fish.EntityPhysicsLogic.Rigidbody.linearDamping * Time.fixedDeltaTime);

                Vector3 nextPosition = position + velocity * Time.fixedDeltaTime;
                Vector3 delta = nextPosition - position;
                
                if (delta != Vector3.zero)
                {
                    int hits = Physics.SphereCastNonAlloc(nextPosition, ((SphereCollider)_fish.EntityPhysicsLogic.Collider).radius, delta.normalized, _markerHitsNonAlloc, delta.magnitude, _settings.MarkerMask);

                    if (hits > 0)
                    {
                        int closestIndex = -1;
                        float closestDistance = Mathf.Infinity;

                        for (int i = 0; i < hits; i++)
                        {
                            if (_markerHitsNonAlloc[i].distance < closestDistance)
                            {
                                closestIndex = i;
                                closestDistance = _markerHitsNonAlloc[i].distance;
                            }
                        }

                        return closestDistance > 0f ? _markerHitsNonAlloc[closestIndex].point : nextPosition;
                    }
                }

                position = nextPosition;
                time += Time.fixedDeltaTime;
            }

            return _fish.EntityPhysicsLogic.Rigidbody.position;
        } 
    }
}