using NoMoreFishAndChips.Audio;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using UnityEngine;
using NoMoreFishAndChips.States;

namespace NoMoreFishAndChips.Entities
{
    public class Seagull : Enemy<SeagullDefinitionData, SeagullSpawnInfo>
    {
        private StateMachine<ESeagullState> _stateMachine;

        private StateAnimationEvents _attackStateAnimationEvents;
        private StateAnimationEvents _airFlapStateAnimationEvents;

        public StateAnimationEvents AttackStateAnimationEvents => _attackStateAnimationEvents;

        private const string IsGroundedBoolName = "IsGrounded";
        private const string InAirBoolName = "InAir";
        private const string InWaterBoolName = "InWater";
        public const string IsFlappingBoolName = "IsFlapping";

        private const string AttackTriggerName = "Attack";

        private const string AttackStateName = "Attack";
        private const string AirFlapStateName = "Base Layer.Air.Flap";

        public override bool TrySpawn(SpawnParams parameters, GameplayContext context, out Enemy enemy)
        {
            enemy = default;

            if (!context.Raft.Queries.TryGetRandomTile(_ => true, out RaftTile tile))
            {
                return false;
            }

            EntityManager entityManager = GameManager.Instance.Get<EntityManager>();
            enemy = (Enemy)entityManager.Spawn(DefinitionData.Id, parameters);

            ((Seagull)enemy).SetSpawnInfo(new SeagullSpawnInfo(tile));

            return true;
        }

        protected override void Awake()
        {
            base.Awake();

            _attackStateAnimationEvents = new StateAnimationEvents(AttackStateName, false);

            _airFlapStateAnimationEvents = new StateAnimationEvents(AirFlapStateName, true)
            {
                new StateAnimationEvent(0.3f, () => _audioManager.PlaySound(SoundId.SeagullFlap))
            };

            _stateMachine = new();

            SeagullAirState airState = new SeagullAirState(_stateMachine, this);

            airState.SubStateMachine.AddState(ESeagullAirState.Takeoff, new SeagullAirTakeoffState(airState.SubStateMachine, this));
            airState.SubStateMachine.AddState(ESeagullAirState.Strafe, new SeagullAirStrafeState(airState.SubStateMachine, this));
            airState.SubStateMachine.AddState(ESeagullAirState.Land, new SeagullAirLandState(airState.SubStateMachine, this));

            SeagullGroundState groundState = new SeagullGroundState(_stateMachine, this);

            groundState.SubStateMachine.AddState(ESeagullGroundState.Idle, new SeagullGroundIdleState(groundState.SubStateMachine, this));
            groundState.SubStateMachine.AddState(ESeagullGroundState.Roam, new SeagullGroundRoamState(groundState.SubStateMachine, this));
            groundState.SubStateMachine.AddState(ESeagullGroundState.Attack, new SeagullGroundAttackState(groundState.SubStateMachine, this));

            _stateMachine.AddState(ESeagullState.Arrive, new SeagullArriveState(_stateMachine, this));
            _stateMachine.AddState(ESeagullState.Air, airState);
            _stateMachine.AddState(ESeagullState.Ground, groundState);
            _stateMachine.AddState(ESeagullState.Water, new SeagullWaterState(_stateMachine, this));
            _stateMachine.AddState(ESeagullState.Stun, new SeagullStunState(_stateMachine, this));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _stateMachine.Dispose();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            EntityDefeatLogic.OnIsDefeatedChanged += HandleIsDefeatedChanged;

            if (isOwner)
            {
                _stateMachine.ChangeState(ESeagullState.Arrive);
            }
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            foreach (ISeagullState state in _stateMachine)
            {
                state.InitialiseContext(_context);
            }
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            EntityDefeatLogic.OnIsDefeatedChanged -= HandleIsDefeatedChanged;

            if (isOwner)
            {
                Cleanup();
            }
        }

        protected override void Update()
        {
            base.Update();

            _entityModel.Animator.SetBool(IsGroundedBoolName, CharacterPhysicsLogic.IsGrounded);
            _entityModel.Animator.SetBool(InAirBoolName, CharacterPhysicsLogic.InAir);
            _entityModel.Animator.SetBool(InWaterBoolName, CharacterPhysicsLogic.InWater);
            
            AnimatorStateInfo info = _entityModel.Animator.GetCurrentAnimatorStateInfo(0);
            _attackStateAnimationEvents.Tick(info);
            _airFlapStateAnimationEvents.Tick(info);

            if (isOwner && isFullySpawned)
            {
                _stateMachine.Tick();
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isOwner && isFullySpawned)
            {
                _stateMachine.FixedTick();
            }
        }

        public void StabiliseAltitude(float dampingStrength)
        {
            if (EntityPhysicsLogic.Rigidbody.linearVelocity.y <= 0f)
            {
                Vector3 force = -Physics.gravity;

                force.y -= EntityPhysicsLogic.Rigidbody.linearVelocity.y * dampingStrength;

                EntityPhysicsLogic.Rigidbody.AddForce(force, ForceMode.Acceleration);
            }
        }

        public void EvaluateState()
        {
            if (CharacterPhysicsLogic.InAir && _stateMachine.CurrentStateEnum != ESeagullState.Air)
            {
                _stateMachine.ChangeState(ESeagullState.Air);
            }
            else if (CharacterPhysicsLogic.IsGrounded && _stateMachine.CurrentStateEnum != ESeagullState.Ground)
            {
                _stateMachine.ChangeState(ESeagullState.Ground);
            }
            else if (CharacterPhysicsLogic.InWater && _stateMachine.CurrentStateEnum != ESeagullState.Water)
            {
                _stateMachine.ChangeState(ESeagullState.Water);
            }
        }

        private void HandleIsDefeatedChanged(bool defeated)
        {
            if (isOwner && defeated)
            {
                Cleanup();
            }
        }

        private void Cleanup()
        {
            if (_stateMachine.CurrentStateEnum != ESeagullState.None)
            {
                _stateMachine.ChangeState(ESeagullState.None);
            }
        }
    }
}