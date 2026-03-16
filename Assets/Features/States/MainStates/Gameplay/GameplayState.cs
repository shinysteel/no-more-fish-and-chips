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
using FishFlingers.Saving;

namespace FishFlingers.States
{
    public class GameplayContext
    {
        public IReadOnlyList<RaftPlayer> Players { get; private set; }
        public RaftPlayer LocalPlayer { get; private set; }
        public Raft Raft { get; private set; }
        public WaveSpawner WaveSpawner { get; private set; }
        public CursorsUI CursorsUI { get; private set; }

        public GameplayContext(List<RaftPlayer> players, RaftPlayer localPlayer, Raft raft, WaveSpawner waveSpawner, CursorsUI cursorsUI)
        {
            Players = players;
            LocalPlayer = localPlayer;
            Raft = raft;
            WaveSpawner = waveSpawner;
            CursorsUI = cursorsUI;
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
        private SaveManager _saveManager;

        private GameplayStateConfig _config;

        private GameplayContext _context;
        private List<RaftPlayer> _players;

        private GameplayScreen _gameplayScreen;
        private CursorsUI _cursorsUI;

        public GameplayState(StateMachine<EMainState> parent) : base(parent)
        {
            _transitionManager = GameManager.Instance.Get<TransitionManager>();
            _uiManager = GameManager.Instance.Get<UIManager>();
            _stateManager = GameManager.Instance.Get<StateManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _sceneManager = GameManager.Instance.Get<SceneManager>();
            _lobbyManager = GameManager.Instance.Get<LobbyManager>();
            _saveManager = GameManager.Instance.Get<SaveManager>();

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

                // All clients need to build a local GameplayContext class
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

                _players = new();

                RaftPlayer localPlayer = _networkManager.LocalPurrnetPlayer.CreateRaftPlayer();

                _context = new GameplayContext(_players, localPlayer, raft, waveSpawner, _cursorsUI);

                // As the client, Any behaviours that spawned before we joined will need to be manually initialised
                if (!_networkManager.IsServer)
                {
                    foreach (GameplayBehaviour behaviour in Object.FindObjectsByType<GameplayBehaviour>(FindObjectsSortMode.None).ToList())
                    {
                        // No need to initialise the localPlayer here. Spawn is async and we are listening for it
                        if (behaviour != localPlayer)
                        {
                            ((INetworkManagerListener)this).OnNetBehaviourSpawned(behaviour);
                        }
                    }
                }

                // Avoid timing issues by making sure the localPlayer is initialised before continuing
                while (!localPlayer.IsInitialised)
                {
                    await Task.Yield();
                }

                _gameplayScreen = await _uiManager.CreateScreenUIAsync(_uiManager.Config.GameplayScreenPrefab, UILayer.Screens);
                _gameplayScreen.Setup(_context);
                _gameplayScreen.Show(null);

                _cursorsUI = await _uiManager.CreateScreenUIAsync(_uiManager.Config.CursorsUIPrefab, UILayer.Cursors);
                _cursorsUI.Show(null);
                _cursorsUI.Setup(_context);

                if (_networkManager.IsServer)
                {
                    _saveManager.LoadGame(_context);
                }

                await _networkManager.LocalPurrnetPlayer.LoadRaftPlayerAsync();

                _transitionManager.UncoverScreen(null);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public override void Exit()
        {
            _context = null;

            _uiManager.DestroyScreenUI(_cursorsUI, UILayer.Cursors);
            _cursorsUI = null;

            _uiManager.DestroyScreenUI(_gameplayScreen, UILayer.Screens);
            _gameplayScreen = null;

            _lobbyManager.LeaveLobby();

            _sceneManager.LoadSceneAsync(EScene.Default, LoadSceneMode.Single, LoadSceneContext.Local);
        }

        void ILobbyManagerListener.OnLobbyEnter(Lobby lobby)
        {
            // This can happen from any state besides itself. Currently we 
            // assume you're 'ready' straight away and move to the GameplayState
            if (_parentStateMachine.CurrentState == this)
            {
                return;
            }

            // Currently we have no lobby flow, and just start the lobby as soon as we create it
            if (_lobbyManager.IsLobbyOwner(lobby))
            {
                _lobbyManager.StartLobby();
            }
        }

        void ILobbyManagerListener.OnLobbyStart(Lobby lobby) 
        {
            if (_parentStateMachine.CurrentState == this)
            {
                return;
            }

            _transitionManager.CoverScreen(() => _stateManager.ChangeState(EMainState.Gameplay));
        }

        void INetworkManagerListener.OnNetworkShutdown(bool asServer)
        {
            if (_parentStateMachine.CurrentState != this)
            {
                return;
            }

            // This will get called twice on the server, as they act as both the server and a client
            if (asServer)
            {
                _saveManager.SaveGame(_context);
            }
        }

        void INetworkManagerListener.OnClientConnectionState(ConnectionState state)
        {
            if (_parentStateMachine.CurrentState != this)
            {
                return;
            }

            if (state != ConnectionState.Disconnected)
            {
                return;
            }

            // This check fixes a very frustrating issue involving the scenario where a host stops the server while other players are connected. What
            // ends up happening is those remaining clients still need to call StopClient, otherwise there will be cookie issues in following sessions.
            // The only reliable way I can detect this is by using reflection for _isSubscribedClient. It's important to to detect this, as StopClient
            // will emit OnClientConnectionState each time its called. The issue is resolved at least, but as a tradeoff this event is getting emitted twice on the clients
            if (_networkManager.IsSubscribedClient)
            {
                _networkManager.StopClient();
            }

            if (!_transitionManager.IsShowing)
            {
                _transitionManager.CoverScreen(() => _stateManager.ChangeState(EMainState.Menus));
            }
        }

        void INetworkManagerListener.OnNetBehaviourSpawned(NetBehaviour behaviour) 
        {
            if (_context == null)
            {
                return;
            }

            if (behaviour is RaftPlayer player)
            {
                _players.Add(player);
            }

            if (behaviour is GameplayBehaviour gameplayBehaviour)
            {
                gameplayBehaviour.Initialise(_context);
            }
        }

        void INetworkManagerListener.OnNetBehaviourDespawned(NetBehaviour behaviour) 
        { 
            if (_context == null)
            {
                return;
            }

            if (behaviour is RaftPlayer player)
            {
                _players.Remove(player);
            }
        }
    }
}