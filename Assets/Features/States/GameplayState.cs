using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Framework;
using FishFlingers.UI.Transitions;
using FishFlingers.UI;
using FishFlingers.Networking;
using System;
using Steamworks;
using ShinyOwl.Common;
using FishFlingers.Scenes;
using System.Threading.Tasks;

namespace FishFlingers.States
{
    public enum EGameplayState { }

    public class GameplayState : State<MainState, EGameplayState>, INetworkManagerListener
    {
        private TransitionManager _transitionManager;
        private UIManager _uiManager;
        private StateManager _stateManager;
        private NetworkManager _networkManager;
        private SceneManager _sceneManager;

        private GameplayScreen _gameplayScreen;

        public GameplayState(StateMachine<MainState> parent) : base(parent)
        {
            _transitionManager = GameManager.Instance.Get<TransitionManager>();
            _uiManager = GameManager.Instance.Get<UIManager>();
            _stateManager = GameManager.Instance.Get<StateManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _sceneManager = GameManager.Instance.Get<SceneManager>();

            _networkManager.AddListener(this);
        }

        ~GameplayState()
        {
            _networkManager?.RemoveListener(this);
        }

        public override void Enter()
        {
            if (_networkManager.CurrentLobby.OwnerId == SteamUser.GetSteamID())
            {
                _networkManager.StartServer();
            }
            
            _networkManager.StartClient();
        }

        public override async Task EnterAsync()
        {
            _gameplayScreen = (GameplayScreen)await _uiManager.CreateUIElementAsync(_uiManager.Config.GameplayScreen, UILayer.Screens);
            _gameplayScreen.Show(null);

            // Use the network manager to load the game scene so that it can be networked
            await _sceneManager.LoadSceneAsync(EScene.Game, LoadSceneMode.Additive, LoadSceneContext.Networked);

            await _sceneManager.LoadSceneAsync(EScene.EnvironmentGameplay, LoadSceneMode.Additive, LoadSceneContext.Local);

            _sceneManager.SetActiveScene(EScene.Game);
            _transitionManager.UncoverScreen(null);
        }

        public override void Exit()
        {
            _uiManager.DestroyUIElement(_gameplayScreen, UILayer.Screens);
            _gameplayScreen = null;

            _networkManager.LeaveLobby();
        }

        public override async Task ExitAsync()
        {
            await _sceneManager.UnloadSceneAsync(EScene.Game);
            await _sceneManager.UnloadSceneAsync(EScene.EnvironmentGameplay);

            _sceneManager.SetActiveScene(EScene.Default);
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