using NoMoreFishAndChips.Audio;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using UnityEngine;
using NoMoreFishAndChips.States;

namespace NoMoreFishAndChips.Entities
{
    public class Seabird : Character<SeabirdDefinitionData>
    {
        private AudioManager _audioManager;

        private StateMachine<ESeabirdState> _stateMachine;

        private RaycastHit[] _glideHitsNonAlloc = new RaycastHit[2];

        private StateAnimationEvents _squawkStateAnimationEvents;
        private StateAnimationEvents _airFlapStateAnimationEvents;

        public StateAnimationEvents SquawkStateAnimationEvents => _squawkStateAnimationEvents;

        private const string EnvironmentStateIntName = "EnvironmentState";
        public const string IsFlappingBoolName = "IsFlapping";
        public const string IsWalkingBoolName = "IsWalking";

        public const string SquawkTriggerName = "Squawk";

        private const string SquawkStateName = "Squawk";
        private const string AirFlapStateName = "Base Layer.Air.Flap";

        protected override void Awake()
        {
            base.Awake();

            _audioManager = GameManager.Instance.Get<AudioManager>();

            _squawkStateAnimationEvents = new StateAnimationEvents(SquawkStateName, false);

            _airFlapStateAnimationEvents = new StateAnimationEvents(AirFlapStateName, true)
            {
                new StateAnimationEvent(0.3f, () => _audioManager.PlaySound(SoundId.SeagullFlap, 0f))
            };

            _stateMachine = new();

            SeabirdAirState airState = new SeabirdAirState(_stateMachine, this);

            airState.SubStateMachine.AddState(ESeabirdAirState.Takeoff, new SeabirdAirTakeoffState(airState.SubStateMachine, this));
            airState.SubStateMachine.AddState(ESeabirdAirState.Strafe, new SeabirdAirStrafeState(airState.SubStateMachine, this));
            airState.SubStateMachine.AddState(ESeabirdAirState.Land, new SeabirdAirLandState(airState.SubStateMachine, this));

            SeabirdGroundState groundState = new SeabirdGroundState(_stateMachine, this);

            groundState.SubStateMachine.AddState(ESeabirdGroundState.Idle, new SeabirdGroundIdleState(groundState.SubStateMachine, this));
            groundState.SubStateMachine.AddState(ESeabirdGroundState.Roam, new SeabirdGroundRoamState(groundState.SubStateMachine, this));
            groundState.SubStateMachine.AddState(ESeabirdGroundState.Squawk, new SeabirdGroundSquawkState(groundState.SubStateMachine, this));

            _stateMachine.AddState(ESeabirdState.Arrive, new SeabirdArriveState(_stateMachine, this));
            _stateMachine.AddState(ESeabirdState.Air, airState);
            _stateMachine.AddState(ESeabirdState.Ground, groundState);
            _stateMachine.AddState(ESeabirdState.Water, new SeabirdWaterState(_stateMachine, this));
            _stateMachine.AddState(ESeabirdState.Stun, new SeabirdStunState(_stateMachine, this));
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
                _stateMachine.ChangeState(ESeabirdState.Arrive);
            }
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            foreach (ISeabirdState state in _stateMachine)
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

            int environmentState = _stateMachine.CurrentStateEnum switch
            {
                ESeabirdState.Ground => 0,
                ESeabirdState.Air or ESeabirdState.Arrive => 1,
                ESeabirdState.Water => 2,
                _ => -1
            };

            _entityModel.Animator.SetInteger(EnvironmentStateIntName, environmentState);
            
            AnimatorStateInfo info = _entityModel.Animator.GetCurrentAnimatorStateInfo(0);
            _squawkStateAnimationEvents.Tick(info);
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

        public bool CanGlide()
        {
            int hits = Utils.Physics.CapsuleCastNonAlloc((CapsuleCollider)_collider, Vector3.zero, Quaternion.identity, Vector3.down, _glideHitsNonAlloc, DefinitionData.GlideDistance, DefinitionData.GlideMask);

            for (int i = 0; i < hits; i++)
            {
                if (_glideHitsNonAlloc[i].collider != _collider)
                {
                    return false;
                }
            }

            return true;
        }

        public void EvaluateState()
        {
            if (CharacterPhysicsLogic.InAir && _stateMachine.CurrentStateEnum != ESeabirdState.Air)
            {
                if (CanGlide())
                {
                    _stateMachine.ChangeState(ESeabirdState.Air);
                }
            }
            else if (CharacterPhysicsLogic.IsGrounded && _stateMachine.CurrentStateEnum != ESeabirdState.Ground)
            {
                _stateMachine.ChangeState(ESeabirdState.Ground);
            }
            else if (CharacterPhysicsLogic.InWater && _stateMachine.CurrentStateEnum != ESeabirdState.Water)
            {
                _stateMachine.ChangeState(ESeabirdState.Water);
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
            if (_stateMachine.CurrentStateEnum != ESeabirdState.None)
            {
                _stateMachine.ChangeState(ESeabirdState.None);
            }
        }
    }
}