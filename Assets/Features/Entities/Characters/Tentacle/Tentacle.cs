using NoMoreFishAndChips.Environments;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class Tentacle : Character<TentacleDefinitionData>
    {
        private StateMachine<EState> _stateMachine;

        private StateAnimationEvents _slamImpactStateAnimationEvents;

        private const string BaseLayerName = "Base Layer";

        private const string IsChargingBoolName = "IsCharging";
        private const string SlamTriggerName = "Slam";

        private const string SlamImpactStateName = BaseLayerName + ".Slam.Impact";

        private enum EState
        {
            None,
            Surface,
            Idle,
            Slam
        }

        private abstract class State : State<EState, ENone>
        {
            protected Tentacle _tentacle;

            public State(StateMachine<EState> parent, Tentacle tentacle) : base(parent)
            {
                _tentacle = tentacle;
            }
        }

        private class SurfaceState : State
        {
            public SurfaceState(StateMachine<EState> parent, Tentacle tentacle) : base(parent, tentacle)
            { }

            public override void Enter()
            {
                base.Enter();

                if (!_tentacle._context.Raft.Queries.TryGetRandomLine(out RaftLine line))
                {
                    _tentacle.OnDespawned();
                    _tentacle._entityManager.Despawn(_tentacle);
                }

                _tentacle.transform.position = _tentacle._context.Raft.Queries.CellToWorldPosition(line.MinEdge.Node.Cell + Utils.Math.DirectionToVector2Int(line.MinEdge.Direction));
                _tentacle.transform.rotation = Quaternion.LookRotation(Utils.Math.DirectionToVector3(line.MaxEdge.Direction), Vector3.up);

                Tween.PositionY(_tentacle.transform, startValue: -3f, endValue: -0.33f, duration: 1f, ease: Ease.OutBack).OnComplete(() => _parentStateMachine.ChangeState(EState.Idle));
            }
        }

        private class IdleState : State
        {
            private float _duration = 4f;

            public IdleState(StateMachine<EState> parent, Tentacle tentacle) : base(parent, tentacle)
            { }

            public override void Tick()
            {
                base.Tick();

                if (_stateTimer >= _duration)
                {
                    _parentStateMachine.ChangeState(EState.Slam);
                }
            }
        }

        private class SlamState : State
        {
            private float _chargeDuration = 4f;
            private int? _markerId;

            public SlamState(StateMachine<EState> parent, Tentacle tentacle) : base(parent, tentacle)
            {
                _tentacle._slamImpactStateAnimationEvents.Add(new StateAnimationEvent(0.5f, RemoveMarker));
            }

            public override void Enter()
            {
                base.Enter();

                _tentacle.CharacterModel.Animator.SetBool(IsChargingBoolName, true);

                _markerId = _tentacle._context.EnvironmentMarker.AddNetMarkedCells(
                    _tentacle._context.Raft.Queries.WorldPositionToCell(_tentacle.transform.position + _tentacle.transform.forward),
                    _tentacle._context.Raft.Queries.WorldPositionToCell(_tentacle.transform.position + _tentacle.transform.forward * 2f));
            }

            public override void Tick()
            {
                if (_stateTimer < _chargeDuration && _stateTimer + Time.deltaTime >= _chargeDuration)
                {
                    _tentacle.CharacterModel.SetTrigger(SlamTriggerName);
                }

                base.Tick();
            }

            public override void Exit()
            {
                base.Exit();

                RemoveMarker();
            }

            private void RemoveMarker()
            {
                if (_tentacle.isOwner && _markerId.HasValue)
                {
                    _tentacle._context.EnvironmentMarker.RemoveNetMarkedCells(_markerId.Value);
                    _markerId = null;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _slamImpactStateAnimationEvents = new StateAnimationEvents(SlamImpactStateName, false)
            {
                new StateAnimationEvent(0f, () =>
                {
                    if (isOwner)
                    {
                        CharacterModel.Animator.SetBool(IsChargingBoolName, false);
                    }
                }),
                new StateAnimationEvent(0.4f, () =>
                {
                    if (isOwner)
                    {
                        _hitboxManager.SpawnHitbox(DefinitionData.SlamHitboxData, new SpawnParams() { Position = transform.position + transform.forward * 1.5f, Rotation = transform.rotation });
                    }
                }),
                new StateAnimationEvent(1f, () =>
                {
                    if (isOwner)
                    {
                        _stateMachine.ChangeState(EState.Idle);
                    }
                })
            };
            
            _stateMachine = new();

            SurfaceState surfaceState = new SurfaceState(_stateMachine, this);
            IdleState idleState = new IdleState(_stateMachine, this);
            SlamState slamState = new SlamState(_stateMachine, this);

            _stateMachine.AddState(EState.Surface, surfaceState);
            _stateMachine.AddState(EState.Idle, idleState);
            _stateMachine.AddState(EState.Slam, slamState);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _stateMachine.Dispose();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            EntityDefeatLogic.OnIsDefeatedChanged += HandleDefeatedChanged;

            if (isOwner)
            {
                _stateMachine.ChangeState(EState.Surface);
            }
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            EntityDefeatLogic.OnIsDefeatedChanged -= HandleDefeatedChanged;
        }

        protected override void Update()
        {
            base.Update();

            AnimatorStateInfo info = _entityModel.Animator.GetCurrentAnimatorStateInfo(0);
            _slamImpactStateAnimationEvents.Tick(info);

            _stateMachine.Tick();
        }

        private void HandleDefeatedChanged(bool defeated)
        {
            if (isOwner)
            {
                _stateMachine.ChangeState(EState.None);
            }
        }
    }
}