using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
using ShinyOwl.Common.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace NoMoreFishAndChips.States
{
    public class StageState : GameplaySubState<ENone>
    {
        private NetworkManager _networkManager;
        private LobbyManager _lobbyManager;
        private EntityManager _entityManager;

        private StageStateConfig _config;

        public StageState(StateMachine<EGameplayState> parent) : base(parent)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _lobbyManager = GameManager.Instance.Get<LobbyManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();
        }

        public override void InitialiseConfig(GameplayStateConfig config)
        {
            _config = config.StageStateConfig;
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            _context.VoyageRunner.OnVoyageResultChanged += HandleVoyageResultChanged;
            _context.VoyageRunner.OnStageComplete += HandleStageComplete;
        }

        public override void Dispose()
        {
            base.Dispose();

            // StageState can be disposed before getting context
            if (_context?.VoyageRunner != null)
            {
                _context.VoyageRunner.OnVoyageResultChanged -= HandleVoyageResultChanged;
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

        private void HandleVoyageResultChanged(VoyageResult result)
        {
            if (!_networkManager.IsServer)
            {
                return;
            }

            if (result != VoyageResult.Defeat)
            {
                return;
            }

            _lobbyManager.StopLobby();

            foreach (Entity entity in _entityManager.Entities.Where(entity => entity is not RaftPlayer).ToList())
            {
                _entityManager.Despawn(entity);
            }

            _parentStateMachine.ChangeState(EGameplayState.Intermission);
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