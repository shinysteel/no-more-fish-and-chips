using NoMoreFishAndChips.Environments;
using ShinyOwl.Common.Framework;
using UnityEngine;
using NoMoreFishAndChips.Networking;

namespace NoMoreFishAndChips.States
{
    public class DepartState : IntermissionSubState
    {
        private NetworkManager _networkManager;

        private IntermissionState _intermissionState;

        private DepartStateConfig _config;

        public DepartState(StateMachine<EIntermissionState> parent, IntermissionState state) : base(parent)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _intermissionState = state;
        }

        public override void InitialiseConfig(IntermissionStateConfig config)
        {
            _config = config.DepartStateConfig;
        }

        public override void Enter()
        {
            base.Enter();

            _context.References.Ocean.SetCurrent(true, Ocean.DefaultSetCurrentDuration);
        }

        public override void Tick()
        {
            base.Tick();

            if (!_networkManager.IsServer)
            {
                return;
            }

            if (_stateTimer >= _config.DepartDelay)
            {
                _intermissionState.GoToStageState();
            }
        }
    }
}