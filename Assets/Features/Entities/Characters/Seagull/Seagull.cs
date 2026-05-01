using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class Seagull : Character<SeagullData>
    {
        private StateMachine<EState> _stateMachine;

        private const string InAirBoolName = "InAir";
        private const string IsFlappingBoolName = "IsFlapping";

        private enum EState
        {
            None,
            Fly,
            Land,
            Idle,
            Poop,
            Takeoff
        }

        private class State : State<EState, ENone>
        {
            protected Seagull _seagull;

            public State(StateMachine<EState> parent) : base(parent)
            { }

            public void Initialise(Seagull seagull)
            {
                _seagull = seagull;
            }
        }

        private class FlyState : State
        {
            private Vector3? _targetPosition;
            private int _targetsChosen;
            private int _glideDirection;
            private Vector3 _velocity;

            public FlyState(StateMachine<EState> parent) : base(parent)
            {
            }

            public override void Enter()
            {
                if (!_seagull._context.Raft.Queries.TryGetRandomTile(out Tile tile))
                {
                    _seagull._entityManager.Despawn(_seagull);
                    return;
                }

                _targetPosition = null;
                _targetsChosen = 0;
                _glideDirection = 0;
                _velocity = Vector3.zero;

                _seagull.transform.position = tile.transform.position;

                Tween.PositionY(_seagull.transform, startValue: 3f, endValue: 2f, duration: 1.5f).OnComplete(NextTarget);
            }

            private void NextTarget()
            {
                if (_targetsChosen > 1)
                {
                    _parentStateMachine.ChangeState(EState.Land);
                    return;
                }
                
                if (_glideDirection == 0)
                {
                    _glideDirection = Random.value < 0.5f ? -1 : 1;
                }
                else
                {
                    _glideDirection = -_glideDirection;
                }

                Vector3 target = _seagull.transform.position + Vector3.right * _glideDirection * Random.Range(3f, 4f);
                target.z += Random.Range(-0.5f, 0.5f);
                target.y += Random.Range(-0.25f, 0.25f);
                _targetPosition = target;

                _targetsChosen++;
            }

            public override void Tick()
            {
                if (!_targetPosition.HasValue)
                {
                    return;
                }

                Vector3 targetDirection = (_targetPosition.Value - _seagull.transform.position).normalized;
                Vector3 targetVelocity = targetDirection * _seagull.Data.FlySettings.Speed;
                Vector3 change = targetVelocity - _velocity;

                _velocity += change * _seagull.Data.FlySettings.Acceleration * Time.deltaTime;
                _seagull.transform.position += _velocity * Time.deltaTime;

                if (Vector3.Distance(_seagull._rigidbody.position, _targetPosition.Value) < 0.1f)
                {
                    NextTarget();
                }
            }
        }

        private class LandState : State
        {
            public LandState(StateMachine<EState> parent) : base(parent)
            {
            }

            public override void Enter()
            {
                if (!_seagull._context.Raft.Queries.TryGetClosestTile(_seagull.transform.position, out Tile tile))
                {
                    _seagull._entityManager.Despawn(_seagull);
                    return;
                }

                _seagull.CharacterModel.Animator.SetBool(IsFlappingBoolName, true);

                Vector3 landPosition = tile.transform.position;
                landPosition.y = tile.GetSurfaceY();

                Tween.Position(_seagull.transform, endValue: landPosition, duration: 2f, ease: Ease.InOutQuad).OnComplete(() => _parentStateMachine.ChangeState(EState.Idle));
            }
        }

        private class IdleState : State
        {
            public IdleState(StateMachine<EState> parent) : base(parent)
            {
            }

            public override void Enter()
            {
                _seagull._rigidbody.isKinematic = false;

                _seagull.CharacterModel.Animator.SetBool(InAirBoolName, false);
            }
        }

        private class PoopState : State
        {
            public PoopState(StateMachine<EState> parent) : base(parent)
            {
            }
        }

        private class TakeoffState : State
        {
            public TakeoffState(StateMachine<EState> parent) : base(parent)
            {
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _stateMachine = new();

            FlyState flyState = new FlyState(_stateMachine);
            LandState landState = new LandState(_stateMachine);
            IdleState idleState = new IdleState(_stateMachine);
            PoopState poopState = new PoopState(_stateMachine);
            TakeoffState takeoffState = new TakeoffState(_stateMachine);

            flyState.Initialise(this);
            landState.Initialise(this);
            idleState.Initialise(this);
            poopState.Initialise(this);
            takeoffState.Initialise(this);

            _stateMachine.AddState(EState.Fly, flyState);
            _stateMachine.AddState(EState.Land, landState);
            _stateMachine.AddState(EState.Idle, idleState);
            _stateMachine.AddState(EState.Poop, poopState);
            _stateMachine.AddState(EState.Takeoff, takeoffState);
        }

        protected override void Update()
        {
            base.Update();

            _stateMachine.Tick();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            _stateMachine.FixedTick();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (isOwner)
            {
                _stateMachine.ChangeState(EState.Fly);
            }
        }
    }
}