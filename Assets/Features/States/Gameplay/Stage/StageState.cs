using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.UI;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using System.Threading.Tasks;
using UnityEngine;

namespace NoMoreFishAndChips.States
{
    public class StageState : GameplaySubState<ENone>
    {
        private NetworkManager _networkManager;

        private StageStateConfig _config;

        public StageState(StateMachine<EGameplayState> parent) : base(parent)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
        }

        public override void InitialiseConfig(GameplayStateConfig config)
        {
            _config = config.StageStateConfig;
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            _context.WaveRunner.OnStageComplete += HandleStageComplete;
        }

        ~StageState()
        {
            if (_context.WaveRunner != null)
            {
                _context.WaveRunner.OnStageComplete -= HandleStageComplete;
            }
        }

        public override void Enter()
        {
            base.Enter();

            if (_networkManager.IsServer)
            {
                _context.WaveRunner.SetStageData(_config.ClamClusterStageData);
            }
        }

        private void HandleStageComplete()
        {
            if (_networkManager.IsServer)
            {
                _parentStateMachine.ChangeState(EGameplayState.Intermission);
            }
        }
    }
}