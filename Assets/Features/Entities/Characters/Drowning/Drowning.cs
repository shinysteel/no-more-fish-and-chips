using PrimeTween;
using PurrNet;
using ShinyOwl.Common.Framework;
using System.Threading.Tasks;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class Drowning : Character<DrowningDefinitionData>, IEntityManagerListener
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private AnimationCurve _scaleCurve;

        private RaftPlayer _targetPlayer;

        private StateMachine<EState> _stateMachine;

        private enum EState
        {
            None,
            Chase,
            Finisher,
            Disappear
        }

        private class State : State<EState, ENone>
        {
            protected Drowning _drowning;

            public State(StateMachine<EState> parent) : base(parent)
            { }

            public void Initialise(Drowning drowning)
            {
                _drowning = drowning;
            }
        }

        private class ChaseState : State
        {
            public ChaseState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Tick()
            {
                if (!_drowning._targetPlayer.RaftPlayerPhysicsModule.InWater)
                {
                    _parentStateMachine.ChangeState(EState.Disappear);
                    return;
                }

                ScaleTick();
                MoveTick();
                DefeatTick();
            }

            private void ScaleTick()
            {
                float speed = Mathf.Pow(1.1f, _drowning._entityLifecycleModule.TimeAlive);
                float time = _drowning._entityLifecycleModule.TimeAlive * speed % 1f;
                float scale = 1f + 0.15f * _drowning._scaleCurve.Evaluate(time);

                _drowning.transform.localScale = Vector3.one * scale;
            }
            
            private void MoveTick()
            {
                Vector3 direction = (_drowning._targetPlayer.transform.position - _drowning.transform.position);
                direction.y = 0f;
                direction.Normalize();

                float speed = -1f + Mathf.Pow(1.4f, _drowning._entityLifecycleModule.TimeAlive);

                _drowning.transform.position += direction * speed * Time.deltaTime;
            }

            private void DefeatTick()
            {
                if (Vector3.Distance(_drowning.transform.position, _drowning._targetPlayer.transform.position) < 0.5f)
                {
                    _drowning._targetPlayer.EntityDefeatModule.SetIsDefeated(true);
                    _parentStateMachine.ChangeState(EState.Finisher);
                }
            }
        }

        private class FinisherState : State
        {
            private float _timer;
            
            public FinisherState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                _timer = 0f;

                Vector3 position = _drowning._targetPlayer.transform.position;
                position.y = _drowning.transform.position.y;

                Tween.Position(_drowning.transform, endValue: position, duration: 0.1f);
                Tween.Scale(_drowning.transform, endValue: 1f, duration: 0.1f);
            }

            public override void Tick()
            {
                _timer += Time.deltaTime;
                
                if (_timer >= 1f)
                {
                    // Since interpolation is enabled, we need to teleport via rigidbody.position
                    _drowning._targetPlayer.RaftPlayerPhysicsModule.Rigidbody.position = new Vector3(Random.Range(-4f, 4f), 0.5f, 5F);

                    _drowning._targetPlayer.RaftPlayerDefeatModule.SetNetInBarrel(true);
                    _parentStateMachine.ChangeState(EState.Disappear);
                }
            }
        }

        private class DisappearState : State
        {
            private float _timer;

            public DisappearState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                _drowning.DisappearRpc();
            }

            public override void Tick()
            {
                _timer += Time.deltaTime;

                if (_timer >= 0.33f)
                {
                    _drowning._entityManager.Despawn(_drowning);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _stateMachine = new();

            ChaseState chaseState = new ChaseState(_stateMachine);
            FinisherState finisherState = new FinisherState(_stateMachine);
            DisappearState disappearState = new DisappearState(_stateMachine);

            chaseState.Initialise(this);
            finisherState.Initialise(this);
            disappearState.Initialise(this);

            _stateMachine.AddState(EState.Chase, chaseState);
            _stateMachine.AddState(EState.Finisher, finisherState);
            _stateMachine.AddState(EState.Disappear, disappearState);
        }

        public void SetTargetPlayer(RaftPlayer targetPlayer)
        {
            _targetPlayer = targetPlayer;

            Vector3 direction = _targetPlayer.transform.position;
            direction.y = 0f;
            direction.Normalize();

            direction = Quaternion.AngleAxis(Random.Range(-30f, 30f), Vector3.up) * direction;

            Vector3 position = _targetPlayer.transform.position;
            position.y = 0f;
            position += direction * 3f;

            transform.position = position;
        }

        protected override void OnEarlySpawn()
        {
            Tween.Alpha(_spriteRenderer, startValue: 0f, endValue: 1f, duration: 0.33f);
        }

        protected override void OnSpawned()
        {
            if (isOwner)
            {
                _entityManager.AddListener(this);

                _stateMachine.ChangeState(EState.Chase);
            }

            base.OnSpawned();
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            if (isOwner)
            {
                _entityManager?.RemoveListener(this);

                _stateMachine.ChangeState(EState.None);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (isOwner)
            {
                _stateMachine.Tick();
            }
        }

        [ObserversRpc]
        private void DisappearRpc()
        {
            Tween.StopAll(_spriteRenderer);
            Tween.Alpha(_spriteRenderer, endValue: 0f, duration: 0.33f);
        }

        void IEntityManagerListener.OnEntityDespawned(IEntity entity)
        {
            if (_stateMachine.CurrentEnum == EState.Disappear)
            {
                return;
            }
            
            if (entity is not RaftPlayer player)
            {
                return;
            }

            if (player == _targetPlayer)
            {
                _stateMachine.ChangeState(EState.Disappear);
            }
        }
    }
}