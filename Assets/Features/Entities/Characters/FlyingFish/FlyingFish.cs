using NoMoreFishAndChips.Environments;
using PurrNet;
using ShinyOwl.Common.Framework;
using UnityEngine;
using ShinyOwl.Common.Utils;
using PrimeTween;
using ShinyOwl.Common.Extensions;
using ShinyOwl.Common;
using NoMoreFishAndChips.Effects;
using NoMoreFishAndChips.Hitboxes;
using NoMoreFishAndChips.Audio;

namespace NoMoreFishAndChips.Entities
{
    public class FlyingFish : Character<FlyingFishDefinitionData>
    {
        private StateMachine<EState> _stateMachine;

        private Tile _targetTile;

        private int? _markerId;

        private const string IsFlyingBoolName = "IsFlying";

        private enum EState
        {
            None,
            Scout,
            Fly
        }

        private class State : State<EState, ENone>
        {
            protected FlyingFish _fish;

            public State(StateMachine<EState> parent) : base(parent)
            { }

            public void Initialise(FlyingFish fish)
            {
                _fish = fish;
            }
        }

        // Surface and scout a tile on the raft
        private class ScoutState : State
        {
            private int _scoutOffset = 3;
            private float _restDistance = 0.05f;
            private float _scoutTilt = 15f;
            private float _surfaceDuration = 0.5f;
            private float _surfaceDistance = 0.5f;

            private float _scoutTimer;
            
            public ScoutState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                _scoutTimer = 0f;

                // Choose a tile to target and a position to scout from
                if (!_fish._context.Raft.Queries.TryGetRandomTile(out _fish._targetTile) 
                    || !_fish._context.Raft.Queries.TryGetClosestEdge(_fish._targetTile.Cell, out RaftEdge edge))
                {
                    _fish._entityManager.Despawn(_fish);
                    return;
                }

                _fish.transform.position = _fish._context.Raft.Queries.CellToWorldPosition(edge.Node.Cell + Utils.Math.DirectionToVector2Int(edge.Direction) * _scoutOffset);

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
                _fish._markerId = _fish._context.EnvironmentMarker.AddNetMarkedCells(_fish._targetTile.Cell);
            }

            public override void Tick()
            {
                _scoutTimer += Time.deltaTime;

                // Scout for some time before attacking
                if (_scoutTimer < _fish.DefinitionData.ScoutDuration)
                {
                    return;
                }

                _parentStateMachine.ChangeState(EState.Fly);
            }
        }

        // Fly into a tile on the raft
        private class FlyState : State
        {
            private Vector3 _anticipatePosition;
            private Quaternion _anticipateRotation;

            private Vector3 _landPosition;
            private Quaternion _landRotation;

            private bool _isAnticipating;
            private float _flyTimer;

            public FlyState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                _flyTimer = 0f;
                
                _isAnticipating = true;
                Vector3 anticipateOffset = Vector3.down * 0.2f;
                float anticipateDuration = 0.2f;

                _anticipatePosition = _fish.transform.position + anticipateOffset;

                // Match the launch angle
                _anticipateRotation = Quaternion.AngleAxis(-_fish.DefinitionData.LaunchAngle, _fish.transform.right) * _fish.transform.rotation;

                // Anticipate with a small duck
                Sequence.Create()
                    .Group(Tween.Position(_fish.transform, _anticipatePosition, anticipateDuration, Ease.OutQuad))
                    .Group(TweenExtensions.Rotation(_fish.transform, _anticipateRotation, anticipateDuration, Ease.OutQuad))
                    .OnComplete(() =>
                    {
                        _isAnticipating = false;
                        _fish.CharacterModel.Animator.SetBool(IsFlyingBoolName, true);
                    });

                _landPosition = _fish._targetTile.transform.position;

                // Straight down
                _landRotation = Quaternion.AngleAxis(90f, _fish.transform.right) * _fish.transform.rotation;
            }
            
            public override void Tick()
            {
                if (_isAnticipating)
                {
                    return;
                }

                _flyTimer += Time.deltaTime;

                // Interpolate from start to end
                float time = _flyTimer / _fish.DefinitionData.FlyDuration;
                _fish.transform.position = Utils.Physics.GetProjectilePosition(_anticipatePosition, _landPosition, Physics.gravity.magnitude, _fish.DefinitionData.LaunchAngle, time);
                _fish.transform.rotation = Quaternion.Slerp(_anticipateRotation, _landRotation, time);

                if (_flyTimer > _fish.DefinitionData.FlyDuration)
                {
                    _fish._hitboxManager.SpawnHitbox(_fish.DefinitionData.ImpactHitboxData, new SpawnParams() { Position = _fish._targetTile.transform.position });

                    _fish._entityManager.Despawn(_fish);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _stateMachine = new();

            ScoutState scoutState = new ScoutState(_stateMachine);
            FlyState flyState = new FlyState(_stateMachine);

            scoutState.Initialise(this);
            flyState.Initialise(this);

            _stateMachine.AddState(EState.Scout, scoutState);
            _stateMachine.AddState(EState.Fly, flyState);
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (isServer)
            {
                _entityDefeatModule.OnIsDefeatedChanged += HandleIsDefeatedChanged;

                _stateMachine.ChangeState(EState.Scout);
            }
        }

        protected override void OnDespawned()
        {
            if (isServer)
            {
                Cleanup();

                _entityDefeatModule.OnIsDefeatedChanged -= HandleIsDefeatedChanged;
            }

            base.OnDespawned();
        }

        protected override void Update()
        {
            base.Update();
            
            if (!isServer)
            {
                return;
            }

            _stateMachine.Tick();
        }

        private void HandleIsDefeatedChanged(bool isDefeated)
        {
            if (isDefeated)
            {
                Cleanup();
            }
        }

        private void Cleanup()
        {
            _targetTile = null;

            if (_markerId.HasValue)
            {
                _context.EnvironmentMarker.RemoveNetMarkedCells(_markerId.Value);
                _markerId = null;
            }

            CharacterModel.Animator.SetBool(IsFlyingBoolName, false);

            // Cleanup will always happen on Despawn, but can also happen when Defeated
            if (_stateMachine.CurrentEnum != EState.None)
            {
                _stateMachine.ChangeState(EState.None);
            }
        }
    }
}