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
        private const string RetreatTriggerName = "Retreat";

        private const string SlamImpactStateName = BaseLayerName + ".Slam.Impact";

        private enum EState
        {
            None,
            Surface,
            Idle,
            Slam,
            Retreat
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

                RaftEdge edge = Random.value < 0.5f ? line.MinEdge : line.MaxEdge;
                _tentacle.transform.position = _tentacle._context.Raft.Queries.CellToWorldPosition(edge.Node.Cell + Utils.Math.DirectionToVector2Int(edge.Direction));
                _tentacle.transform.rotation = Quaternion.LookRotation(-Utils.Math.DirectionToVector3(edge.Direction), Vector3.up);

                Tween.PositionY(_tentacle.transform, startValue: -3f, endValue: -0.33f, duration: 1f, ease: Ease.OutBack).OnComplete(() => _parentStateMachine.ChangeState(EState.Idle));
            }
        }

        private class IdleState : State
        {
            private float _duration = 3f;

            public IdleState(StateMachine<EState> parent, Tentacle tentacle) : base(parent, tentacle)
            { }

            public override void Enter()
            {
                base.Enter();

                TryRetreat();
            }

            public override void Tick()
            {
                base.Tick();

                if (_stateTimer >= _duration)
                {
                    if (!TryRetreat())
                    {
                        _parentStateMachine.ChangeState(EState.Slam);
                    }
                }
            }

            private bool TryRetreat()
            {
                if (_tentacle._context.Raft.Tiles.ContainsKey(_tentacle._context.Raft.Queries.WorldPositionToCell(_tentacle.transform.position + _tentacle.transform.forward))
                    || _tentacle._context.Raft.Tiles.ContainsKey(_tentacle._context.Raft.Queries.WorldPositionToCell(_tentacle.transform.position + _tentacle.transform.forward * 2f)))
                {
                    return false;
                }
                else
                {
                    _parentStateMachine.ChangeState(EState.Retreat);
                    return true;
                }
            }
        }

        private class SlamState : State
        {
            private float _chargeDuration = 3f;
            private int? _markerId;

            public SlamState(StateMachine<EState> parent, Tentacle tentacle) : base(parent, tentacle)
            {
                _tentacle._slamImpactStateAnimationEvents.Add(new StateAnimationEvent(0.5f, RemoveMarker));
            }

            public override void Enter()
            {
                base.Enter();

                _tentacle.CharacterModel.Animator.SetBool(IsChargingBoolName, true);

                //_markerId = _tentacle._context.EnvironmentMarker.AddNetMarkedCells(
                //    _tentacle._context.Raft.Queries.WorldPositionToCell(_tentacle.transform.position + _tentacle.transform.forward),
                //    _tentacle._context.Raft.Queries.WorldPositionToCell(_tentacle.transform.position + _tentacle.transform.forward * 2f));
            }

            public override void Tick()
            {
                if (!_tentacle.CharacterActLogic.CanAct)
                {
                    _parentStateMachine.ChangeState(EState.Idle);
                }
                else if (_stateTimer < _chargeDuration && _stateTimer + Time.deltaTime >= _chargeDuration)
                {
                    _tentacle.CharacterModel.SetTrigger(SlamTriggerName);
                }

                base.Tick();
            }

            public override void Exit()
            {
                base.Exit();

                _tentacle.CharacterModel.Animator.SetBool(IsChargingBoolName, false);

                RemoveMarker();
            }

            private void RemoveMarker()
            {
                if (_tentacle.isOwner && _markerId.HasValue)
                {
                    // _tentacle._context.EnvironmentMarker.RemoveNetMarkedCells(_markerId.Value);
                    _markerId = null;
                }
            }
        }

        private class RetreatState : State
        {
            public RetreatState(StateMachine<EState> parent, Tentacle tentacle) : base(parent, tentacle)
            { }

            public override void Enter()
            {
                _tentacle.CharacterModel.SetTrigger(RetreatTriggerName);

                Tween.PositionY(_tentacle.transform, endValue: -3f, duration: 1f, ease: Ease.InBack)
                    .OnComplete(() => _tentacle._entityManager.Despawn(_tentacle));
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _slamImpactStateAnimationEvents = new StateAnimationEvents(SlamImpactStateName, false)
            {
                new StateAnimationEvent(0.4f, () =>
                {
                    if (isOwner)
                    {
                        _hitboxManager.SpawnHitbox(DefinitionData.SlamHitboxData, this, new SpawnParams() { Position = transform.position + transform.forward * 1.5f, Rotation = transform.rotation });
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
            RetreatState retreatState = new RetreatState(_stateMachine, this);
            
            _stateMachine.AddState(EState.Surface, surfaceState);
            _stateMachine.AddState(EState.Idle, idleState);
            _stateMachine.AddState(EState.Slam, slamState);
            _stateMachine.AddState(EState.Retreat, retreatState);
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