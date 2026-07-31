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
        private LobbyManager _lobbyManager;

        private StageStateConfig _config;

        public StageState(StateMachine<EGameplayState> parent) : base(parent)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _lobbyManager = GameManager.Instance.Get<LobbyManager>();
        }

        public override void InitialiseConfig(GameplayStateConfig config)
        {
            _config = config.StageStateConfig;
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            _context.VoyageRunner.OnStageComplete += HandleStageComplete;
        }

        ~StageState()
        {
            if (_context.VoyageRunner != null)
            {
                _context.VoyageRunner.OnStageComplete -= HandleStageComplete;
            }
        }

        public override void Enter()
        {
            base.Enter();

            if (_networkManager.IsServer)
            {
                _context.VoyageRunner.ContinueVoyage();
            }
        }
        
        private void HandleStageComplete()
        {
            if (!_networkManager.IsServer)
            {
                return;
            }

            if (_context.VoyageRunner.VoyageResult != VoyageResult.None)
            {
                _lobbyManager.StopLobby();
            }

            _parentStateMachine.ChangeState(EGameplayState.Intermission);
        }
    }
}