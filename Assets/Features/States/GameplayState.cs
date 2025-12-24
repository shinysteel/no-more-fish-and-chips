using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Framework;
using UnityEngine.SceneManagement;
using FishFlingers.UI.Transitions;
using FishFlingers.UI;
using FishFlingers.Networking;
using System;
using Steamworks;
using ShinyOwl.Common;

namespace FishFlingers.States
{
    public enum EGameplayState { }

    public class GameplayState : State<MainState, EGameplayState>, INetworkManagerListener
    {
        private TransitionManager _transitionManager;
        private UIManager _uiManager;
        private StateManager _stateManager;
        private NetworkManager _networkManager;

        private GameplayScreen _gameplayScreen;

        public GameplayState(StateMachine<MainState> parent) : base(parent)
        {
            _transitionManager = GameManager.Instance.Get<TransitionManager>();
            _uiManager = GameManager.Instance.Get<UIManager>();
            _stateManager = GameManager.Instance.Get<StateManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _networkManager.AddListener(this);
        }

        ~GameplayState()
        {
            _networkManager?.RemoveListener(this);
        }

        public override void Enter()
        {
            _gameplayScreen = _uiManager.CreateUIElementInLayer(_uiManager.Config.GameplayScreen, UILayer.Screens);
            _gameplayScreen.Show(null);

            // Load the environment
            AsyncOperation op = SceneManager.LoadSceneAsync(SceneRegistry.GetSceneName(EScene.EnvironmentGameplay), LoadSceneMode.Additive);
            op.completed += _ =>
            {
                SceneManager.SetActiveScene(SceneRegistry.GetScene(EScene.EnvironmentGameplay));
                _transitionManager.UncoverScreen(null);
            };

            if (_networkManager.CurrentLobby.OwnerId == SteamUser.GetSteamID())
            {
                _networkManager.StartServer();
            }
            
            _networkManager.StartClient();
        }

        public override void Exit()
        {
            _uiManager.DestroyUIElementInLayer(_gameplayScreen, UILayer.Screens);
            _gameplayScreen = null;

            SceneManager.UnloadSceneAsync(SceneRegistry.GetSceneName(EScene.EnvironmentGameplay));

            _networkManager.LeaveLobby();
        }

        public void OnLobbyEnter(SteamLobby lobby)
        {
            // This can happen from any state besides itself. Currently we 
            // assume you are 'ready' straight away and move to the GameplayState
            if (_parentStateMachine.CurrentState == this)
            {
                return;
            }

            if (lobby.OwnerId == SteamUser.GetSteamID())
            {
                _networkManager.StartLobby();
            }
            else
            {
                // For now, let's skip the check for the lobby being active
                OnLobbyGameServerSet();
            }
        }

        // This fires once the lobby has 'started'
        public void OnLobbyGameServerSet() 
        {
            if (_parentStateMachine.CurrentState == this)
            {
                return;
            }

            _transitionManager.CoverScreen(() => _stateManager.ChangeState(MainState.Gameplay));
        }

        public void OnNetworkShutdown(bool asServer)
        {
            if (_parentStateMachine.CurrentState != this)
            {
                return;
            }

            // This will get called twice on the server, as they act as both the server and a client
            if (asServer)
            {
                return;
            }

            _transitionManager.CoverScreen(() => _stateManager.ChangeState(MainState.Menus));
        }

        public void OnLobbyLeave()  { }
        public void OnLobbyCreated(SteamLobby lobby) { }
        public void OnPlayerJoined(PurrNet.PlayerID id, bool isReconnect) { }
        public void OnPlayerLeft(PurrNet.PlayerID id) { }
    }
}