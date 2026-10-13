using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.States;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class Tentacle : Character<TentacleDefinitionData>
    {
        private StateMachine<ETentacleState> _stateMachine;

        private StateAnimationEvents _slamChargeStateAnimationEvents;
        public StateAnimationEvents SlamChargeStateAnimationEvents => _slamChargeStateAnimationEvents;

        private StateAnimationEvents _slamImpactStateAnimationEvents;
        public StateAnimationEvents SlamImpactStateAnimationEvents => _slamImpactStateAnimationEvents;

        private Vector3 _slamDirection;
        public Vector3 SlamDirection => _slamDirection;

        private const string BaseLayerName = "Base Layer";

        public const string IsChargingBoolName = "IsCharging";

        public const string SlamTriggerName = "Slam";

        private const string SlamChargeStateName = BaseLayerName + ".Slam.Charge";
        private const string SlamImpactStateName = BaseLayerName + ".Slam.Impact";

        protected override void Awake()
        {
            base.Awake();
            
            // canLoop is set to false so that an SFX only plays once
            _slamChargeStateAnimationEvents = new StateAnimationEvents(SlamChargeStateName, false);

            _slamImpactStateAnimationEvents = new StateAnimationEvents(SlamImpactStateName, false);
            
            _stateMachine = new();

            _stateMachine.AddState(ETentacleState.Arrive, new TentacleArriveState(_stateMachine, this));
            _stateMachine.AddState(ETentacleState.Approach, new TentacleApproachState(_stateMachine, this));
            _stateMachine.AddState(ETentacleState.Slam, new TentacleSlamState(_stateMachine, this));
            _stateMachine.AddState(ETentacleState.Stun, new TentacleStunState(_stateMachine, this));
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
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            foreach (TentacleState state in _stateMachine)
            {
                state.InitialiseContext(_context);
            }

            if (isOwner)
            {
                _stateMachine.ChangeState(ETentacleState.Arrive);
            }
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            EntityDefeatLogic.OnIsDefeatedChanged -= HandleDefeatedChanged;

            if (isOwner)
            {
                Cleanup();
            }
        }

        protected override void Update()
        {
            base.Update();

            AnimatorStateInfo info = _entityModel.Animator.GetCurrentAnimatorStateInfo(0);

            _slamChargeStateAnimationEvents.Tick(info);
            _slamImpactStateAnimationEvents.Tick(info);

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

        public void SetSlamDirection(Vector3 direction)
        {
            _slamDirection = direction;
        }

        private void HandleDefeatedChanged(bool defeated)
        {
            if (isOwner && defeated)
            {
                Cleanup();
            }
        }

        private void Cleanup()
        {
            if (_stateMachine.CurrentStateEnum != ETentacleState.None)
            {
                _stateMachine.ChangeState(ETentacleState.None);
            }
        }
    }
}