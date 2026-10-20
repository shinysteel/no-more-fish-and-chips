using NoMoreFishAndChips.Environments;
using PrimeTween;
using ShinyOwl.Common.Framework;
using UnityEngine;
using NoMoreFishAndChips.Networking;
using ShinyOwl.Common;

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
            if (_networkManager.IsServer)
            {
                _context.Ocean.SetCurrent(false, Ocean.SetCurrentDuration);

                Tween.Delay(Ocean.SetCurrentDuration, () => _parentStateMachine.ChangeState(EIntermissionState.Dock));
            }
        }
    }
}