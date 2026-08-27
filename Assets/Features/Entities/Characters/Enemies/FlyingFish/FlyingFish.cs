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
using NoMoreFishAndChips.States;

namespace NoMoreFishAndChips.Entities
{
    public class FlyingFish : Character<FlyingFishDefinitionData>
    {
        private StateMachine<EFlyingFishState> _stateMachine;

        private Vector3 _landPosition;
        public Vector3 LandPosition => _landPosition;
        
        private int? _markerId;

        public const string IsFlyingBoolName = "IsFlying";

        protected override void Awake()
        {
            base.Awake();

            _stateMachine = new();

            FlyingFishSurfaceState surfaceState = new FlyingFishSurfaceState(_stateMachine, this);
            FlyingFishFlyState flyState = new FlyingFishFlyState(_stateMachine, this);

            _stateMachine.AddState(EFlyingFishState.Surface, surfaceState);
            _stateMachine.AddState(EFlyingFishState.Fly, flyState);
        } 

        protected override void OnSpawned()
        {
            base.OnSpawned();

            EntityDefeatLogic.OnIsDefeatedChanged += HandleIsDefeatedChanged;
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            foreach (FlyingFishState state in _stateMachine)
            {
                state.InitialiseContext(context);
            }

            _stateMachine.ChangeState(EFlyingFishState.Surface);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            EntityDefeatLogic.OnIsDefeatedChanged -= HandleIsDefeatedChanged;

            if (isServer)
            {
                Cleanup();
            } 
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _stateMachine.Dispose();
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

        public void SetLandPosition(Vector3 position)
        {
            _landPosition = position;
        }

        public void SetMarkerId(int? id)
        {
            _markerId = id;
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
            _landPosition = Vector3.zero;

            if (_markerId.HasValue)
            {
                _context.EnvironmentMarker.RemoveNetMarkedCells(_markerId.Value);
                _markerId = null;
            }

            _entityModel.Animator.SetBool(IsFlyingBoolName, false);

            // Cleanup will always happen on Despawn, but can also happen when Defeated
            if (_stateMachine.CurrentStateEnum != EFlyingFishState.None)
            {
                _stateMachine.ChangeState(EFlyingFishState.None);
            }
        }
    }
}