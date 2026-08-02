using NoMoreFishAndChips.Cameras;
using NoMoreFishAndChips.Effects;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Instantiating;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Saving;
using NoMoreFishAndChips.Scenes;
using NoMoreFishAndChips.UI;
using NoMoreFishAndChips.UI.Transitions;
using PurrNet;
using PurrNet.Transports;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using ShinyOwl.Common.Utils;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;
using Object = UnityEngine.Object;

namespace NoMoreFishAndChips.States
{
    public class GameplayContext
    {
        public IReadOnlyList<RaftPlayer> Players { get; private set; }
        public RaftPlayer LocalPlayer { get; private set; }
        public Raft Raft { get; private set; }
        public VoyageRunner VoyageRunner { get; private set; }
        public EnvironmentMarker EnvironmentMarker { get; private set; }
        public EnvironmentGameplayReferences References { get; private set; }
        public GameplayScreen GameplayScreen { get; private set; }
        
        public GameplayContext(List<RaftPlayer> players, RaftPlayer localPlayer, Raft raft, VoyageRunner voyageRunner, EnvironmentMarker environmentMarker, EnvironmentGameplayReferences references, GameplayScreen gameplayScreen)
        {
            Players = players;
            LocalPlayer = localPlayer;
            Raft = raft;
            VoyageRunner = voyageRunner;
            EnvironmentMarker = environmentMarker;
            References = references;
            GameplayScreen = gameplayScreen;
        }
    }

    public enum EGameplayState
    {
        None,
        Intermission,
        Stage
    }

    public interface IGameplaySubState
    {
        void InitialiseContext(GameplayContext context);
    }

    public abstract class GameplaySubState<TSubStateEnum> : State<EGameplayState, TSubStateEnum>, IGameplaySubState
        where TSubStateEnum : Enum
    {
        protected GameplayContext _context;

        public GameplaySubState(StateMachine<EGameplayState> parent) : base(parent)
        { }

        public virtual void InitialiseConfig(GameplayStateConfig config)
        { }

        public virtual void InitialiseContext(GameplayContext context)
        {
            _context = context;
        }
    }

    public class GameplayState : MainState<EMainState, EGameplayState>, ILobbyManagerListener, INetworkManagerListener, IInstantiateManagerListener
    {
        private TransitionManager _transitionManager;
        private UIManager _uiManager;
        private StateManager _stateManager;
        private NetworkManager _networkManager;
        private SceneManager _sceneManager;
        private LobbyManager _lobbyManager;
        private SaveManager _saveManager;
        private CameraManager _cameraManager;
        private InstantiateManager _instantiateManager;

        private GameplayStateConfig _config;

        private CancellationTokenSource _enterCTS;

        private GameplayContext _context;
        private List<RaftPlayer> _players;
        private bool _isContextReady;

        private GameplayScreen _gameplayScreen;
        private CursorsUI _cursorsUI;

        private FollowCameraMode _followCameraMode;

        public GameplayState(StateMachine<EMainState> parent) : base(parent)
        {
            _transitionManager = GameManager.Instance.Get<TransitionManager>();
            _uiManager = GameManager.Instance.Get<UIManager>();
            _stateManager = GameManager.Instance.Get<StateManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _sceneManager = GameManager.Instance.Get<SceneManager>();
            _lobbyManager = GameManager.Instance.Get<LobbyManager>();
            _saveManager = GameManager.Instance.Get<SaveManager>();
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _instantiateManager = GameManager.Instance.Get<InstantiateManager>();

            _networkManager.AddListener(this);
            _lobbyManager.AddListener(this);
            _instantiateManager.AddListener(this);
        }

        public override void Dispose()
        {
            base.Dispose();

            _networkManager?.RemoveListener(this);
            _lobbyManager?.RemoveListener(this);
            _instantiateManager?.RemoveListener(this);

            _enterCTS?.Cancel();
            _enterCTS?.Dispose();
        }

        public override void Initialise(StateManagerConfig config)
        {
            _config = config.GameplayStateConfig;

            _subStateMachine = new();

            IntermissionState lobbyState = new IntermissionState(_subStateMachine);
            StageState stageState = new StageState(_subStateMachine);

            lobbyState.InitialiseConfig(_config);
            stageState.InitialiseConfig(_config);

            _subStateMachine.AddState(EGameplayState.Intermission, lobbyState);
            _subStateMachine.AddState(EGameplayState.Stage, stageState);
        }

        public override void Enter()
        {
            base.Enter();

            _enterCTS = new CancellationTokenSource();
            _ = EnterAsync(_enterCTS.Token);
        }

        private async Task EnterAsync(CancellationToken token)
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

                if (token.IsCancellationRequested)
                {
                    return;
                }

                await _sceneManager.LoadSceneAsync(EScene.EnvironmentGameplay, LoadSceneMode.Additive, LoadSceneContext.Local);

                EnvironmentGameplayReferences references = Object.FindFirstObjectByType<EnvironmentGameplayReferences>();

                // All clients need to build a local GameplayContext class
                Raft raft = null;
                VoyageRunner voyageRunner = null;
                EnvironmentMarker environmentMarker = null;

                if (_networkManager.IsServer)
                {
                    raft = _networkManager.Spawn(_config.RaftPrefab);
                    voyageRunner = _networkManager.Spawn(_config.VoyageRunnerPrefab);
                    environmentMarker = _networkManager.Spawn(_config.EnvironmentMarkerPrefab);

                    _networkManager.Spawn(_config.DrowningSpawnerPrefab);
                    _networkManager.Spawn(_config.SalvageSpawnerPrefab);
                }
                else
                {
                    // Clients will need to retrieve these objects
                    while (raft == null || voyageRunner == null || environmentMarker == null)
                    {
                        raft ??= Object.FindFirstObjectByType<Raft>();
                        voyageRunner ??= Object.FindFirstObjectByType<VoyageRunner>();
                        environmentMarker ??= Object.FindFirstObjectByType<EnvironmentMarker>();
                        await Task.Yield();
                    }
                }

                _players = new();
                RaftPlayer localPlayer = _networkManager.LocalPurrnetPlayer.CreateRaftPlayer();

                _gameplayScreen = await _uiManager.CreateScreenUIAsync(_uiManager.Config.GameplayScreenPrefab, UILayer.Screens);

                _context = new GameplayContext(_players, localPlayer, raft, voyageRunner, environmentMarker, references, _gameplayScreen);

                // Manually initialise context components to dictate order
                InitialiseComponent(raft);
                InitialiseComponent(voyageRunner);
                InitialiseComponent(environmentMarker);

                InitialiseComponent(localPlayer);

                _gameplayScreen.Setup(_context);
                _gameplayScreen.Show(null);

                foreach (IGameplaySubState state in _subStateMachine)
                {
                    state.InitialiseContext(_context);
                }

                // Initialising the states is the final step before context can be marked as ready, since StateSynchroniser exists
                _isContextReady = true;

                // CursorsUI listens for OnEntitySpawned, which is not simultaneous with RaftPlayers being initialised and added to the players list 
                _cursorsUI = await _uiManager.CreateScreenUIAsync(_uiManager.Config.CursorsUIPrefab, UILayer.Cursors);
                _cursorsUI.Setup(_context);
                _cursorsUI.Show(null);

                // The server will setup an environment object
                if (_networkManager.IsServer)
                {
                    GameplayEnvironment environment = Object.Instantiate(_config.GameplayEnvironmentPrefab);
                    environment.Initialise(_context);
                }

                if (_networkManager.IsServer)
                {
                    await _saveManager.LoadGameAsync();
                }
                else
                {
                    await ((ISaveable)_networkManager.LocalPurrnetPlayer).LoadAsync();
                }
                
                // Before switching the camera, we need to wait a physics step for the player to be positioned correctly
                await Utils.Tasks.WaitForFixedUpdateAsync();

                _followCameraMode = new FollowCameraMode(_cameraManager.Config.RaftPlayerFollowCameraModeSettings, _context.LocalPlayer.transform);
                _cameraManager.AddMode(_followCameraMode);

                if (_networkManager.IsServer)
                {
                    _subStateMachine.ChangeState(EGameplayState.Intermission);
                }

                _transitionManager.UncoverScreen(null);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public override void Exit()
        {
            base.Exit();

            _context = null;
            _isContextReady = false;

            _uiManager.DestroyScreenUI(_cursorsUI, UILayer.Cursors);
            _cursorsUI = null;

            _uiManager.DestroyScreenUI(_gameplayScreen, UILayer.Screens);
            _gameplayScreen = null;

            _subStateMachine.ChangeState(EGameplayState.None);

            _lobbyManager.LeaveLobby();

            _sceneManager.LoadSceneAsync(EScene.Default, LoadSceneMode.Single, LoadSceneContext.Local);

            _cameraManager.RemoveMode(_followCameraMode);
            _followCameraMode = null;
        }

        void ILobbyManagerListener.OnLobbyEnter(Lobby lobby)
        {            
            // This can happen from any state besides itself. Currently we 
            // assume you're 'ready' straight away and move to the GameplayState
            if (_parentStateMachine.CurrentState == this)
            {
                return;
            }

            _transitionManager.CoverScreen(() => _stateManager.ChangeMainState(EMainState.Gameplay));
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
                _transitionManager.CoverScreen(() => _stateManager.ChangeMainState(EMainState.Menus));
            }
        }

        void INetworkManagerListener.OnNetBehaviourSpawned(NetBehaviour behaviour) 
        {
            _ = InitialiseComponentAsync(behaviour);
        }

        void IInstantiateManagerListener.OnComponentInstantiated(Component component)
        {
            _ = InitialiseComponentAsync(component);
        }

        private async Task InitialiseComponentAsync(Component component)
        {
            try
            {
                while (!_isContextReady)
                {
                    await Task.Yield();
                }

                InitialiseComponent(component);
                
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void InitialiseComponent(Component component)
        {
            // Anything in context will go through this twice, since they are manually initialised
            if (component is RaftPlayer player && !_players.Contains(player))
            {
                _players.Add(player);
            }

            if (component is IRequiresGameplayContext requires && !requires.IsContextInitialised)
            {
                requires.InitialiseContext(_context);
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