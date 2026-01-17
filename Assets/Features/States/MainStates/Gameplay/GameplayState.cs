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
using PurrNet.Transports;
using PurrNet;
using FishFlingers.Environments;
using FishFlingers.Entities;
using System.Linq;

using NetworkManager = FishFlingers.Networking.NetworkManager;
using Object = UnityEngine.Object;

namespace FishFlingers.States
{
    public class GameplayContext
    {
        public List<RaftPlayer> Players { get; private set; }
        public RaftPlayer LocalPlayer { get; private set; }
        public Raft Raft { get; private set; }
        public WaveSpawner WaveSpawner { get; private set; }

        public GameplayContext(List<RaftPlayer> players, RaftPlayer localPlayer, Raft raft, WaveSpawner waveSpawner)
        {
            Players = players;
            LocalPlayer = localPlayer;
            Raft = raft;
            WaveSpawner = waveSpawner;
        }
    }

    public class GameplayState : MainState<EMainState, ENone>, ILobbyManagerListener, INetworkManagerListener
    {
        private TransitionManager _transitionManager;
        private UIManager _uiManager;
        private StateManager _stateManager;
        private NetworkManager _networkManager;
        private SceneManager _sceneManager;
        private LobbyManager _lobbyManager;

        private GameplayStateConfig _config;

        private List<RaftPlayer> _players;

        private GameplayScreen _gameplayScreen;

        public GameplayState(StateMachine<EMainState> parent) : base(parent)
        {
            _transitionManager = GameManager.Instance.Get<TransitionManager>();
            _uiManager = GameManager.Instance.Get<UIManager>();
            _stateManager = GameManager.Instance.Get<StateManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _sceneManager = GameManager.Instance.Get<SceneManager>();
            _lobbyManager = GameManager.Instance.Get<LobbyManager>();

            _networkManager.AddListener(this);
            _lobbyManager.AddListener(this);
        }

        ~GameplayState()
        {
            _networkManager?.RemoveListener(this);
            _lobbyManager?.RemoveListener(this);
        }

        public override void Initialise(StateManagerConfig config)
        {
            _config = config.GameplayStateConfig;
        }
        
        public override async Task EnterAsync()
        {
            try
            {
                if (_networkManager.IsServer)
                {
                    // Network the game scene
                    await _sceneManager.LoadSceneAsync(EScene.Game, LoadSceneMode.Single, LoadSceneContext.Networked);
                }
                else
                {
                    // Scenes are structs, so we need to keep requesting while awaiting
                    while (!_sceneManager.IsSceneActive(EScene.Game))
                    {
                        await Task.Yield();
                    }
                }

                await _sceneManager.LoadSceneAsync(EScene.EnvironmentGameplay, LoadSceneMode.Additive, LoadSceneContext.Local);

                Raft raft = null;
                WaveSpawner waveSpawner = null;
                SalvageSpawner salvageSpawner = null;

                if (_networkManager.IsServer)
                {
                    raft = _networkManager.Spawn(_config.RaftPrefab);
                    waveSpawner = _networkManager.Spawn(_config.WaveSpawnerPrefab);
                    salvageSpawner = _networkManager.Spawn(_config.SalvageSpawnerPrefab);
                }
                else
                {
                    // Clients will need to retrieve these objects
                    while (raft == null || waveSpawner == null || salvageSpawner == null)
                    {
                        raft ??= Object.FindFirstObjectByType<Raft>();
                        waveSpawner ??= Object.FindFirstObjectByType<WaveSpawner>();
                        salvageSpawner ??= Object.FindFirstObjectByType<SalvageSpawner>();
                        await Task.Yield();
                    }
                }

                // After initially retrieving all players, OnNetworkSpawn & OnNetworkDespawn will maintain it
                _players = Object.FindObjectsByType<RaftPlayer>(FindObjectsSortMode.None).ToList();

                RaftPlayer localPlayer = _networkManager.Spawn(_config.RaftPlayerPrefab, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition });

                GameplayContext context = new GameplayContext(_players, localPlayer, raft, waveSpawner);

                // Inject context into everything that needs it
                raft.Initialise(context);
                waveSpawner.Initialise(context);
                salvageSpawner.Initialise(context);
                localPlayer.Initialise(context);

                _gameplayScreen = (GameplayScreen)await _uiManager.CreateScreenUIAsync(_uiManager.Config.GameplayScreen, UILayer.Screens);
                _gameplayScreen.Setup(context);
                _gameplayScreen.Show(null);

                _transitionManager.UncoverScreen(null);
            }
            catch (Exception ex)
            {
                Debugger.LogError(this, ex);
            }
        }

        public override void Exit()
        {
            _players = null;

            _uiManager.DestroyScreenUI(_gameplayScreen, UILayer.Screens);
            _gameplayScreen = null;

            _lobbyManager.LeaveLobby();

            _sceneManager.LoadSceneAsync(EScene.Default, LoadSceneMode.Single, LoadSceneContext.Local);
        }

        public void OnLobbyEnter(Lobby lobby)
        {
            // This can happen from any state besides itself. Currently we 
            // assume you are 'ready' straight away and move to the GameplayState
            if (_parentStateMachine.CurrentEnum == EMainState.Gameplay)
            {
                return;
            }

            // Currently we have no lobby flow, and just start the lobby as soon as we create it
            if (_lobbyManager.IsLobbyOwner(lobby))
            {
                _lobbyManager.StartLobby();
            }
        }

        public void OnLobbyStart(Lobby lobby) 
        {
            if (_parentStateMachine.CurrentEnum == EMainState.Gameplay)
            {
                return;
            }

            _transitionManager.CoverScreen(() => _stateManager.ChangeState(EMainState.Gameplay));
        }

        public void OnNetworkShutdown(bool asServer)
        {
            if (_parentStateMachine.CurrentEnum != EMainState.Gameplay)
            {
                return;
            }

            // This will get called twice on the server, as they act as both the server and a client
            if (asServer)
            {
                return;
            }

            _transitionManager.CoverScreen(() => _stateManager.ChangeState(EMainState.Menus));
        }

        public void OnNetworkSpawn(NetBehaviour behaviour) 
        { 
            // Players is not null when we are in the GameplayState
            if (_players == null)
            {
                return;
            }

            if (behaviour is not RaftPlayer player)
            {
                return;
            }

            _players.Add(player);
        }

        public void OnNetworkDespawn(NetBehaviour behaviour) 
        { 
            if (_players == null)
            {
                return;
            }

            if (behaviour is not RaftPlayer player)
            {
                return;
            }

            _players.Remove(player);
        }

        public void OnLobbyLeave() { }
        public void OnLobbyCreated(Lobby lobby) { }
        public void OnPlayerJoined(PlayerID id, bool isReconnect, bool asServer) { }
        public void OnPlayerLeft(PlayerID id, bool asServer) { }
        public void OnClientConnectionState(ConnectionState state) { }
        public void OnNetworkStarted(bool asServer) { }
    }
}