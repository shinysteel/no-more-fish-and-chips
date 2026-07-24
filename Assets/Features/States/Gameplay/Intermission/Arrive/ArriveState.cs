using NoMoreFishAndChips.Environments;
using PrimeTween;
using ShinyOwl.Common.Framework;
using UnityEngine;
using NoMoreFishAndChips.Networking;

namespace NoMoreFishAndChips.States
{
    public class ArriveState : IntermissionSubState
    {
        private NetworkManager _networkManager;

        private ArriveStateConfig _config;

        public ArriveState(StateMachine<EIntermissionState> parent) : base(parent)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
        }

        public override void InitialiseConfig(IntermissionStateConfig config)
        {
            _config = config.ArriveStateConfig;
        }

        public override void Enter()
        {
            base.Enter();

            Tween.Delay(_config.ArriveDelay, Arrive);
        }

        private void Arrive()
        {
            _context.References.Ocean.SetCurrent(false, Ocean.DefaultSetCurrentDuration);

            if (_networkManager.IsServer)
            {
                Tween.Delay(Ocean.DefaultSetCurrentDuration, () => _parentStateMachine.ChangeState(EIntermissionState.Dock));
            }
        }
    }
}