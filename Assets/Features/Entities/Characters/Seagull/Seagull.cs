using FishFlingers.Audio;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Extensions;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class Seagull : Character<SeagullDefinitionData>
    {
        private StateMachine<EState> _stateMachine;

        private StateAnimationEvents _attackStateAnimationEvents;

        private const string InAirBoolName = "InAir";
        private const string IsFlappingBoolName = "IsFlapping";

        private const string AttackTriggerName = "Attack";

        private const string AttackStateName = "Attack";

        private enum EState
        {
            None,
            Fly,
            Land,
            Idle,
            Poop,
            Attack,
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
            { }

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

                TweenExtensions.Rotation(_seagull.transform, endValue: Quaternion.AngleAxis(_glideDirection * -10f, Vector3.forward), duration: 0.5f, Ease.OutQuad);
            }

            public override void Tick()
            {
                if (!_targetPosition.HasValue)
                {
                    return;
                }

                Vector3 targetDirection = (_targetPosition.Value - _seagull.transform.position).normalized;
                Vector3 targetVelocity = targetDirection * _seagull.DefinitionData.FlySettings.Speed;
                Vector3 change = targetVelocity - _velocity;

                _velocity += change * _seagull.DefinitionData.FlySettings.Acceleration * Time.deltaTime;
                _seagull.transform.position += _velocity * Time.deltaTime;

                if (Vector3.Distance(_seagull._rigidbody.position, _targetPosition.Value) < 0.1f)
                {
                    NextTarget();
                }
            }
        }

        private class LandState : State
        {
            private Vector3 _landPosition;

            public LandState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                if (!_seagull._context.Raft.Queries.TryGetClosestTile(_seagull.transform.position, out Tile tile))
                {
                    _seagull._entityManager.Despawn(_seagull);
                    return;
                }

                _seagull.CharacterModel.Animator.SetBool(IsFlappingBoolName, true);

                _landPosition = tile.transform.position;
                _landPosition.y = tile.GetSurfaceY();

                _seagull._rigidbody.isKinematic = false;

                TweenExtensions.Rotation(_seagull.transform, endValue: Quaternion.LookRotation(Vector3.forward, Vector3.up), duration: 1f, ease: Ease.OutQuad);
            }

            public override void FixedTick()
            {
                if (_seagull.CharacterPhysicsModule.IsGrounded)
                {
                    _parentStateMachine.ChangeState(EState.Idle);
                    return;
                }

                _seagull.CharacterPhysicsModule.Rigidbody.AddForce(Vector3.up * 7f, ForceMode.Acceleration);

                Vector3 direction = (_landPosition - _seagull.transform.position);
                direction.y = 0f;
                direction.Normalize();

                _seagull.CharacterPhysicsModule.Rigidbody.AddForce(direction * 1f, ForceMode.Acceleration);
            }
        }

        private class IdleState : State
        {
            private Collider[] _collidersNonAlloc = new Collider[1];

            public IdleState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                _seagull.CharacterModel.Animator.SetBool(InAirBoolName, false);
            }

            public override void FixedTick()
            {
                if (Physics.OverlapSphereNonAlloc(_seagull.transform.position, _seagull.DefinitionData.AttackSettings.Range, _collidersNonAlloc, _seagull.DefinitionData.AttackSettings.Mask) > 0)
                {
                    Vector3 direction = (_collidersNonAlloc[0].transform.position - _seagull.transform.position);
                    direction.y = 0f;
                    direction.Normalize();

                    TweenExtensions.Rotation(_seagull.transform, endValue: Quaternion.LookRotation(direction, Vector3.up), duration: 0.2f, ease: Ease.OutQuad);

                    _parentStateMachine.ChangeState(EState.Attack);
                }
            }
        }

        private class PoopState : State
        {
            public PoopState(StateMachine<EState> parent) : base(parent)
            { }
        }

        private class AttackState : State
        {
            public AttackState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                _seagull.CharacterModel.SetTrigger(AttackTriggerName);
            }
        }

        private class TakeoffState : State
        {
            public TakeoffState(StateMachine<EState> parent) : base(parent)
            { }
        }

        protected override void Awake()
        {
            base.Awake();

            _stateMachine = new();

            FlyState flyState = new FlyState(_stateMachine);
            LandState landState = new LandState(_stateMachine);
            IdleState idleState = new IdleState(_stateMachine);
            PoopState poopState = new PoopState(_stateMachine);
            AttackState attackState = new AttackState(_stateMachine);
            TakeoffState takeoffState = new TakeoffState(_stateMachine);

            flyState.Initialise(this);
            landState.Initialise(this);
            idleState.Initialise(this);
            poopState.Initialise(this);
            attackState.Initialise(this);
            takeoffState.Initialise(this);

            _stateMachine.AddState(EState.Fly, flyState);
            _stateMachine.AddState(EState.Land, landState);
            _stateMachine.AddState(EState.Idle, idleState);
            _stateMachine.AddState(EState.Poop, poopState);
            _stateMachine.AddState(EState.Attack, attackState);
            _stateMachine.AddState(EState.Takeoff, takeoffState);
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            _attackStateAnimationEvents = new StateAnimationEvents(AttackStateName, false)
            {
                new StateAnimationEvent(0.3f, () => _audioManager.PlaySound(SoundId.SeagullAttack)),   
            };

            if (isOwner)
            {
                _attackStateAnimationEvents.Add(new StateAnimationEvent(0.3f, () => _hitboxManager.SpawnHitbox(DefinitionData.AttackSettings.HitboxData, new SpawnParams() { Position = transform.position })));
                _attackStateAnimationEvents.Add(new StateAnimationEvent(0.3f, () => CharacterPhysicsModule.Rigidbody.AddForce(Vector3.up * 10f, ForceMode.Impulse)));
                _attackStateAnimationEvents.Add(new StateAnimationEvent(1f, () => _stateMachine.ChangeState(EState.Idle)));

                _stateMachine.ChangeState(EState.Fly);
                
                CharacterDefeatModule.OnIsDefeatedChanged += HandleIsDefeatedChanged;
            }
        }

        protected override void OnDespawned()
        {
            if (isOwner)
            {
                CharacterDefeatModule.OnIsDefeatedChanged -= HandleIsDefeatedChanged;
            }

            base.OnDespawned();
        }

        protected override void Update()
        {
            base.Update();

            if (!isFullySpawned)
            {
                return;
            }

            AnimatorStateInfo info = CharacterModel.Animator.GetCurrentAnimatorStateInfo(0);
            _attackStateAnimationEvents.Tick(info);

            if (!isOwner)
            {
                return;
            }
            
            _stateMachine.Tick();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!isFullySpawned)
            {
                return;
            }

            if (!isOwner)
            {
                return;
            }

            _stateMachine.FixedTick();
        }

        private void HandleIsDefeatedChanged(bool defeated)
        {
            _stateMachine.ChangeState(EState.None);
        }
    }
}