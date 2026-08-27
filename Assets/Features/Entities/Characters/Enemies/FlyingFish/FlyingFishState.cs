using NoMoreFishAndChips.Effects;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Hitboxes;
using NoMoreFishAndChips.States;
using PrimeTween;
using ShinyOwl.Common.Extensions;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using UnityEngine;

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

    // Surface and scout a tile on the raft
    public class FlyingFishSurfaceState : FlyingFishState
    {
        private int _scoutOffset = 3;
        private float _restDistance = 0.05f;
        private float _scoutTilt = 15f;
        private float _surfaceDuration = 0.5f;
        private float _surfaceDistance = 0.5f;

        public FlyingFishSurfaceState(StateMachine<EFlyingFishState> parent, FlyingFish fish) : base(parent, fish)
        { }

        public override void Enter()
        {
            base.Enter();

            // Choose a tile to target and a position to scout from
            if (!_context.Raft.Queries.TryGetRandomTile((RaftTile tile) => !tile.EntityDefeatLogic.IsDefeated, out RaftTile tile)
                || !_context.Raft.Queries.TryGetClosestEdge(tile.Cell, out RaftEdge edge))
            {
                // _fish.OnDespawned();
                // _fish._entityManager.Despawn(_fish);
                return;
            }

            _fish.SetLandPosition(_context.Raft.Queries.CellToWorldPosition(tile.Cell));

            _fish.transform.position = _context.Raft.Queries.CellToWorldPosition(edge.Node.Cell + Utils.Math.DirectionToVector2Int(edge.Direction) * _scoutOffset);

            // Rest slightly in the water
            _fish.transform.position += Vector3.down * _restDistance;

            // Face towards the raft, with a slight tilt up
            _fish.transform.rotation = Quaternion.LookRotation(-Utils.Math.DirectionToVector3(edge.Direction));
            _fish.transform.rotation = Quaternion.AngleAxis(-_scoutTilt, _fish.transform.right) * _fish.transform.rotation;

            // Animate from underwater to surface
            _fish.transform.position += Vector3.down * _surfaceDistance;
            Vector3 surfacePosition = _fish.transform.position + Vector3.up * _surfaceDistance;
            Tween.Position(_fish.transform, endValue: surfacePosition, duration: _surfaceDuration, ease: Ease.OutBack);

            EffectManager.SpawnVfxRpc(VfxId.WaterSplash, new Vector3(surfacePosition.x, 0f, surfacePosition.z));

            // Place a marker
            _fish.SetMarkerId(_context.EnvironmentMarker.AddNetMarkedCells(tile.Cell));
        }

        public override void Tick()
        {
            base.Tick();

            // Scout for some time before attacking
            if (_stateTimer < _fish.DefinitionData.ScoutDuration)
            {
                return;
            }

            _parentStateMachine.ChangeState(EFlyingFishState.Fly);
        }
    }

    // Fly into a tile on the raft
    public class FlyingFishFlyState : FlyingFishState
    {
        private HitboxManager _hitboxManager;
        private EntityManager _entityManager;

        private Vector3 _anticipatePosition;
        private Quaternion _anticipateRotation;

        private Quaternion _landRotation;

        private bool _isAnticipating;
        private float _flyTimer;

        public FlyingFishFlyState(StateMachine<EFlyingFishState> parent, FlyingFish fish) : base(parent, fish)
        {
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();
        }

        public override void Enter()
        {
            base.Enter();

            _flyTimer = 0f;

            _isAnticipating = true;
            Vector3 anticipateOffset = Vector3.down * 0.2f;
            float anticipateDuration = 0.2f;

            _anticipatePosition = _fish.transform.position + anticipateOffset;

            // Match the launch angle
            _anticipateRotation = Quaternion.AngleAxis(-_fish.DefinitionData.LaunchAngle, _fish.transform.right) * _fish.transform.rotation;

            // Anticipate with a small duck
            Sequence.Create()
                .Chain(Tween.Position(_fish.transform, _anticipatePosition, anticipateDuration, Ease.OutQuad))
                .Group(TweenExtensions.Rotation(_fish.transform, _anticipateRotation, anticipateDuration, Ease.OutQuad))
                .OnComplete(() =>
                {
                    _isAnticipating = false;
                    _fish.EntityModel.Animator.SetBool(FlyingFish.IsFlyingBoolName, true);
                });

            // Straight down
            _landRotation = Quaternion.AngleAxis(90f, _fish.transform.right) * _fish.transform.rotation;
        }

        public override void Tick()
        {
            base.Tick();

            if (_isAnticipating)
            {
                return;
            }

            _flyTimer += Time.deltaTime;

            // Interpolate from start to end
            float time = _flyTimer / _fish.DefinitionData.FlyDuration;
            _fish.transform.position = Utils.Physics.GetProjectilePosition(_anticipatePosition, _fish.LandPosition, Physics.gravity.magnitude * 0.5f, _fish.DefinitionData.LaunchAngle, time);
            _fish.transform.rotation = Quaternion.Slerp(_anticipateRotation, _landRotation, time);

            if (_flyTimer > _fish.DefinitionData.FlyDuration)
            {
                _hitboxManager.SpawnHitbox(_fish.DefinitionData.ImpactHitboxData, _fish, new SpawnParams() { Position = _fish.LandPosition });

                _entityManager.Despawn(_fish);
            }
        }
    }
}