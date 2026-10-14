using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Effects;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Hitboxes;
using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoMoreFishAndChips.Entities
{
    public enum ETentacleState
    {
        None,
        Arrive,
        Approach,
        Slam,
        Stun
    }

    public abstract class TentacleState : State<ETentacleState, ENone>
    {
        protected Tentacle _tentacle;
        protected GameplayContext _context;

        public TentacleState(StateMachine<ETentacleState> parent, Tentacle tentacle) : base(parent)
        {
            _tentacle = tentacle;
        }

        public void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }
    }

    public class TentacleArriveState : TentacleState
    {
        private TentacleArriveSettings _settings;

        public TentacleArriveState(StateMachine<ETentacleState> parent, Tentacle tentacle) : base(parent, tentacle)
        {
            _settings = _tentacle.DefinitionData.ArriveSettings;
        }

        public override void Enter()
        {
            base.Enter();

            Surface();
        }

        private void Surface()
        {
            _context.Raft.Queries.TryGetRandomLine(out RaftLine line);

            RaftEdge edge = line.GetRandomEdge();

            Vector2Int cell = edge.Node.Tile.Cell;
            Vector3 position = _context.Raft.Queries.TileCellToWorldPosition(cell);

            position += edge.Direction3d * 0.5f;
            position += edge.Direction3d * _tentacle.CharacterPhysicsLogic.CapsuleCollider.radius;

            position += Vector3.Cross(edge.Direction3d, Vector3.up) * (Random.value - 0.5f);
            position += Vector3.down * _settings.Depth;
            
            _tentacle.EntityPhysicsLogic.Rigidbody.position = position;

            _tentacle.EntityPhysicsLogic.Rigidbody.rotation = Quaternion.LookRotation(-edge.Direction3d, Vector3.up);
        }

        public override void Tick()
        {
            base.Tick();

            if (_stateTimer >= _settings.Duration)
            {
                _parentStateMachine.ChangeState(ETentacleState.Approach);
            }
        }
    }

    public class TentacleApproachState : TentacleState
    {
        private TentacleApproachSettings _settings;

        private Collider[] _tileOverlapsNonAlloc = new Collider[4];

        public TentacleApproachState(StateMachine<ETentacleState> parent, Tentacle tentacle) : base(parent, tentacle)
        {
            _settings = _tentacle.DefinitionData.ApproachSettings;
        }

        public override void Enter()
        {
            base.Enter();

            // If we are already facing a tile, go straight to slam state
            if (CanSlam())
            {
                _tentacle.SetSlamDirection(_tentacle.transform.forward);
                _parentStateMachine.ChangeState(ETentacleState.Slam);
            }
            // Otherwise, determine how to approach the raft
            else
            {
                _tentacle.CharacterNavigateLogic.StartNavigation(_context.EnvironmentGameplayReferences.Ocean, DetermineTargetPosition(),
                    onTrySetPath: (bool set) =>
                    {
                        if (!set)
                        {
                            _tentacle.CharacterNavigateLogic.SetTargetPosition(DetermineTargetPosition());
                        }
                    },
                    onPathStale: () => _tentacle.CharacterNavigateLogic.SetTargetPosition(DetermineTargetPosition()),
                    onDestinationReached: () => _parentStateMachine.ChangeState(ETentacleState.Slam));
            }
        }

        private bool CanSlam()
        {
            Vector3 center = _tentacle.transform.position;
            center += _tentacle.transform.forward * 0.5f;
            center += Vector3.up * 0.25f;

            Vector3 halfExtents = new Vector3(0.25f, 0.5f, 0.5f);

            int overlaps = Physics.OverlapBoxNonAlloc(center, halfExtents, _tileOverlapsNonAlloc, _tentacle.transform.rotation, _settings.TileMask);

            for (int i = 0; i < overlaps; i++)
            {
                if (!Utils.Physics.ColliderTryGetComponent(_tileOverlapsNonAlloc[i], out RaftTile tile))
                {
                    continue;
                }

                if (!tile.EntityDefeatLogic.IsDefeated)
                {
                    return true;
                }
            }

            return false;
        }

        private Vector2 DetermineTargetPosition()
        {
            // Find the closest perimeter node. Note: there is a scenario where all remaining tiles are 'defeated'
            if (!_context.Raft.Queries.TryGetClosestPerimeterNode(_tentacle.transform.position, (RaftPerimeterNode node) => !node.Tile.EntityDefeatLogic.IsDefeated, out RaftPerimeterNode node))
            {
                return Vector2.zero;
            }

            // Find the closest side
            float closestDistance = Mathf.Infinity;
            Vector3 closestDirection3d = Vector3.zero;

            foreach (Vector3 direction in node.OpenDirections)
            {
                Vector3 position = node.Tile.transform.position;
                position.y = _tentacle.transform.position.y;
                position += direction * 0.5f;
                float distance = Vector3.Distance(position, _tentacle.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDirection3d = direction;
                }
            }

            _tentacle.SetSlamDirection(-closestDirection3d);

            // Approach from that side
            Vector3 targetPosition = _context.Raft.Queries.TileCellToWorldPosition(node.Tile.Cell);
            targetPosition += closestDirection3d * 0.5f;
            targetPosition += closestDirection3d * _tentacle.CharacterPhysicsLogic.CapsuleCollider.radius;

            // Add some perpendicular randomness
            Vector2Int closestDirection2d = new Vector2Int((int)closestDirection3d.x, (int)closestDirection3d.z);
            Vector2Int targetCell = node.Tile.Cell + closestDirection2d;
            Vector3 perpendicularDirection3d = Vector3.Cross(closestDirection3d, Vector3.up);
            Vector2Int perpendicularDirection2d = new Vector2Int((int)perpendicularDirection3d.x, (int)perpendicularDirection3d.z);

            float perpendicularMin = !_context.Raft.Tiles.ContainsKey(targetCell - perpendicularDirection2d) ? -0.5f : -0.5f + _tentacle.CharacterPhysicsLogic.CapsuleCollider.radius;
            float perpendicularMax = !_context.Raft.Tiles.ContainsKey(targetCell + perpendicularDirection2d) ? 0.5f : 0.5f - _tentacle.CharacterPhysicsLogic.CapsuleCollider.radius;

            perpendicularMin = Mathf.Min(perpendicularMin, 0f);
            perpendicularMax = Mathf.Max(perpendicularMax, 0f);

            targetPosition += perpendicularDirection3d * Random.Range(perpendicularMin, perpendicularMax);

            return _context.Raft.Queries.WorldPositionToStructurePosition(targetPosition);
        }

        public override void Exit()
        {
            base.Exit();

            _tentacle.CharacterNavigateLogic.StopNavigation();
        }
    }

    public class TentacleSlamState : TentacleState
    {
        private HitboxManager _hitboxManager;
        private AudioManager _audioManager;

        private TentacleSlamSettings _settings;

        private NetMarkerHandle _netMarkerHandle;

        public TentacleSlamState(StateMachine<ETentacleState> parent, Tentacle tentacle) : base(parent, tentacle)
        {
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();
            _audioManager = GameManager.Instance.Get<AudioManager>();

            _settings = _tentacle.DefinitionData.SlamSettings;

            _tentacle.SlamChargeStateAnimationEvents.Add(new StateAnimationEvent(0f, () =>
            {
                if (_tentacle.isOwner)
                {
                    _netMarkerHandle = _context.EnvironmentMarker.CreateNetMarker(_tentacle.transform.position + _tentacle.transform.forward * 1.25f, _tentacle.transform.rotation, new Vector3(0.5f, 1f, 1.5f), 0f);
                }

                _audioManager.PlaySound(SoundId.TentacleCharge, 0f);
            }));

            _tentacle.SlamImpactStateAnimationEvents.Add(new StateAnimationEvent(0.33f, () =>
            {
                if (_tentacle.isOwner)
                {
                    _hitboxManager.SpawnHitbox(_settings.HitboxData, _tentacle, new SpawnParams() { Position = _tentacle.transform.position + _tentacle.transform.forward, Rotation = _tentacle.transform.rotation });

                    RemoveMarker();
                }

                _audioManager.PlaySound(SoundId.TentacleSlam, 0f);
            }));

            _tentacle.SlamImpactStateAnimationEvents.Add(new StateAnimationEvent(1f, () =>
            {
                if (_tentacle.isOwner)
                {
                    _parentStateMachine.ChangeState(ETentacleState.Approach);
                }
            }));
        }

        public override void Enter()
        {
            base.Enter();

            _tentacle.EntityModel.Animator.SetBool(Tentacle.IsChargingBoolName, true);
        }

        public override void Tick()
        {
            base.Tick();

            _netMarkerHandle?.SetPosition(_tentacle.transform.position + _tentacle.transform.forward);
            _netMarkerHandle?.SetRotation(_tentacle.transform.rotation);

            if (_stateTimer - Time.deltaTime < _settings.ChargeDuration && _stateTimer >= _settings.ChargeDuration)
            {
                _tentacle.EntityModel.SetAnimatorTrigger(Tentacle.SlamTriggerName);
                _tentacle.EntityModel.Animator.SetBool(Tentacle.IsChargingBoolName, false);
            }
        }

        public override void FixedTick()
        {
            base.FixedTick();

            _tentacle.CharacterPhysicsLogic.Rotate(_tentacle.SlamDirection, _settings.RotateSpeed);
        }

        public override void Exit()
        {
            base.Exit();

            RemoveMarker();
        }

        private void RemoveMarker()
        {
            _netMarkerHandle?.Remove();
            _netMarkerHandle = null;
        }
    }

    public class TentacleStunState : TentacleState
    {
        public TentacleStunState(StateMachine<ETentacleState> parent, Tentacle tentacle) : base(parent, tentacle)
        { }
    }
}