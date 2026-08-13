using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Saving;
using ShinyOwl.Common.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Pool;
using System.Threading.Tasks;
using PurrNet;
using ShinyOwl.Common;

using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;
using System;

namespace NoMoreFishAndChips.States
{
    public class StageState : GameplaySubState<ENone>
    {
        private NetworkManager _networkManager;
        private LobbyManager _lobbyManager;
        private EntityManager _entityManager;
        private SaveManager _saveManager;

        private StageStateConfig _config;

        public StageState(StateMachine<EGameplayState> parent) : base(parent)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _lobbyManager = GameManager.Instance.Get<LobbyManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _saveManager = GameManager.Instance.Get<SaveManager>();
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

        public override void Tick()
        {
            bool result = true;

            foreach (Entity entity in _entityManager.Entities)
            {
                if (entity == null)
                {
                    result = false;
                }
            }

            if (result == false)
            {
                Log.Info($"theres a null in entities");
            }
        }
        
        private void HandleStageComplete()
        {
            if (!_networkManager.IsServer)
            {
                return;
            }

            if (_context.VoyageRunner.VoyageResult == VoyageResult.None)
            {
                _parentStateMachine.ChangeState(EGameplayState.Intermission);
            }
        }

        private void HandleVoyageResultChanged(VoyageResult result)
        {
            if (!_networkManager.IsServer)
            {
                return;
            }

            if (result != VoyageResult.None)
            {
                _ = RestartGameAsync();
            }
        }

        private async Task RestartGameAsync()
        {
            try
            {
                _lobbyManager.StopLobby();

                foreach (Entity entity in _entityManager.Entities.Where(entity => entity is not RaftPlayer).ToArray())
                {
                    _entityManager.Despawn(entity);
                }

                await _saveManager.LoadGameAsync();

                LoadPlayerRpc();

                _parentStateMachine.ChangeState(EGameplayState.Intermission);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
        
        [ObserversRpc]
        public static void LoadPlayerRpc()
        {
            NetworkManager networkManager = GameManager.Instance.Get<NetworkManager>();

            if (networkManager.IsServer)
            {
                return;
            }

            ((ISaveable)networkManager.LocalPurrnetPlayer).LoadAsync();
        }
    }
}