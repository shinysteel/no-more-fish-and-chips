using PurrLobby;
using PurrNet;
using PurrNet.Modules;
using PurrNet.Transports;
using ShinyOwl.Common;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net.Sockets;
using PurrNet.Steam;
using System.Linq;
using System;

namespace FishFlingers.Networking
{
    public enum eTransport
    {
        UDP   ,
        Steam ,
    }

    public enum eLobbyService
    {
        None  ,
        LAN   ,
        Steam , 
    }

    public interface INetworkManagerListener
    {
        void OnLobbyCreated(Lobby lobby);
        void OnLobbyEnter(Lobby lobby);
        void OnLobbyStart(Lobby lobby);
        void OnLobbyLeave();
        void OnNetworkStarted(bool asServer);
        void OnNetworkShutdown(bool asServer);
        void OnClientConnectionState(ConnectionState state);
        void OnPlayerJoined(PlayerID id, bool isReconnect, bool asServer);
        void OnPlayerLeft(PlayerID id, bool asServer);
    }

    public class NetworkManager : GameSystem<INetworkManagerListener>
    {
        private NetworkManagerConfig _config;
        public NetworkManagerConfig Config => _config;

        private PurrNet.NetworkManager _purrnetNetworkManager;

        private LANLobbyService _lanLobbyService;
        private SteamLobbyService _steamLobbyService;

        private Dictionary<eLobbyService, LobbyService> _lobbyServices = new();
        private LobbyService _currentLobbyService;

        public Lobby CurrentLobby => _currentLobbyService.CurrentLobby;
        public PlayerID LocalPlayer => _purrnetNetworkManager.localPlayer;

        public bool IsServer => _purrnetNetworkManager.isServer;

        public override void Initialise(GameManagerConfig gameManagerConfig)
        {
            _config = gameManagerConfig.NetworkManagerConfig;

            _lanLobbyService = new();
            _steamLobbyService = new();

            _lobbyServices.Add(eLobbyService.None, null);
            _lobbyServices.Add(eLobbyService.LAN, _lanLobbyService);
            _lobbyServices.Add(eLobbyService.Steam, _steamLobbyService);

            _purrnetNetworkManager = UnityEngine.Object.Instantiate(_config.PurrnetNetworkManagerPrefab);
            _purrnetNetworkManager.onNetworkStarted += HandleNetworkStarted;
            _purrnetNetworkManager.onNetworkShutdown += HandleNetworkShutdown;
            _purrnetNetworkManager.onClientConnectionState += HandleClientConnectionState;
            _purrnetNetworkManager.onPlayerJoined += HandlePlayerJoined;
            _purrnetNetworkManager.onPlayerLeft += HandlePlayerLeft;

            GetTransport<UDPTransport>().serverPort = _config.UDPServerPort;
            GetTransport<SteamTransport>().serverPort = _config.SteamServerPort;

            // Remove this and let other scripts request
            SetClientTransport<UDPTransport>();
            SetLobbyService(eLobbyService.LAN);

            base.Initialise(gameManagerConfig);
        }

        public override void Shutdown()
        {
            _currentLobbyService.Shutdown();
            SetLobbyService(eLobbyService.None);   

            _purrnetNetworkManager.onNetworkStarted -= HandleNetworkStarted;
            _purrnetNetworkManager.onNetworkShutdown -= HandleNetworkShutdown;
            _purrnetNetworkManager.onClientConnectionState -= HandleClientConnectionState;
            _purrnetNetworkManager.onPlayerJoined -= HandlePlayerJoined;
            _purrnetNetworkManager.onPlayerLeft -= HandlePlayerLeft;

            base.Shutdown();
        }

        // Our transport will always be composite, so it is a safe cast
        private CompositeTransport GetCompositeTransport()
        {
            return (CompositeTransport)_purrnetNetworkManager.currentTransport;
        }

        private T GetTransport<T>() where T : GenericTransport
        {
            GetCompositeTransport().TryGetTransport(out T transport);
            return (T)transport;
        }

        // We use a try here since the requested transport could potentially not be what the client is using
        public bool TryGetClientTransport<T>(out T transport) where T : GenericTransport
        {
            transport = GetCompositeTransport().clientTransport as T;
            return transport != null;
        }

        public void SetClientTransport<T>() where T : GenericTransport
        {
            GetCompositeTransport().SetClientTransport<T>();
        }

        public void SetLobbyService(eLobbyService service)
        {
            if (_currentLobbyService != null)
            {
                _currentLobbyService.OnLobbyCreated -= HandleLobbyCreated;
                _currentLobbyService.OnLobbyEnter -= HandleLobbyEnter;
                _currentLobbyService.OnLobbyStart -= HandleLobbyStart;
                _currentLobbyService.OnLobbyLeave -= HandleLobbyLeave;
            }

            if (!_lobbyServices.TryGetValue(service, out _currentLobbyService))
            {
                Debugger.LogError(this, "Trying to set a lobby service that is not defined");
            }

            // Lobby service should never be null after the first time it's set 
            if (_currentLobbyService != null)
            {
                _currentLobbyService.OnLobbyCreated += HandleLobbyCreated;
                _currentLobbyService.OnLobbyEnter += HandleLobbyEnter;
                _currentLobbyService.OnLobbyStart += HandleLobbyStart;
                _currentLobbyService.OnLobbyLeave += HandleLobbyLeave;
            }
        }

        public AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode)
        {
            return _purrnetNetworkManager.sceneModule.LoadSceneAsync(sceneName, mode);
        }

        public AsyncOperation UnloadSceneAsync(string sceneName)
        {
            return _purrnetNetworkManager.sceneModule.UnloadSceneAsync(sceneName);
        }

        public void Send<T>(PlayerID id, T data, Channel method = Channel.ReliableUnordered)
        {
            _purrnetNetworkManager.Send(id, data, method);
        }

        public void Subscribe<T>(PlayerBroadcastDelegate<T> callback, bool asServer) where T : new()
        {
            _purrnetNetworkManager.Subscribe(callback, asServer);
        }

        public void Unsubscribe<T>(PlayerBroadcastDelegate<T> callback) where T : new()
        {
            _purrnetNetworkManager.Unsubscribe(callback);
        }

        public void KickPlayer(PlayerID id)
        {
            _purrnetNetworkManager.playerModule.KickPlayer(id);
        }

        public async Task<Dictionary<eLobbyService, Lobby[]>> SearchLobbies()
        {
            var tasks = new List<Task<KeyValuePair<eLobbyService, Lobby[]>>>();

            foreach (var kvp in _lobbyServices)
            {
                if (kvp.Key == eLobbyService.None)
                {
                    continue;
                }

                tasks.Add(Task.Run(async () => new KeyValuePair<eLobbyService, Lobby[]>(kvp.Key, await kvp.Value.SearchLobbiesAsync())));
            }

            KeyValuePair<eLobbyService, Lobby[]>[] kvps = await Task.WhenAll(tasks);

            return kvps.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task<Lobby> CreateLobbyAsync()
        {
            Lobby lobby = await _currentLobbyService.CreateLobbyAsync();
            return lobby;
        }

        public async Task<Lobby> JoinLobbyAsync(string lobbyId)
        {
            Lobby lobby = await _currentLobbyService.JoinLobbyAsync(lobbyId);
            return lobby;
        }

        public void StartLobby()
        {
            _currentLobbyService.StartLobby();
        }

        public void LeaveLobby()
        {
            _currentLobbyService.LeaveLobby();
        }

        public bool IsLobbyOwner(Lobby lobby)
        {
            return _currentLobbyService.IsLobbyOwner(lobby);
        }

        public void StartServer()
        {
            _purrnetNetworkManager.StartServer();
        }

        public void StartClient()
        {
            _purrnetNetworkManager.StartClient();
        }

        public void StopServer()
        {
            _purrnetNetworkManager.StopServer();
        }

        public void StopClient()
        {
            _purrnetNetworkManager.StopClient();
        }

        private void HandleLobbyCreated(Lobby lobby) => Listeners.Dispatch(NotifyOnLobbyCreated, lobby);
        private void HandleLobbyEnter(Lobby lobby) => Listeners.Dispatch(NotifyOnLobbyEnter, lobby);
        private void HandleLobbyStart(Lobby lobby) => Listeners.Dispatch(NotifyOnLobbyStart, lobby);
        private void HandleLobbyLeave() => Listeners.Dispatch(NotifyOnLobbyLeave);
        private void HandleNetworkStarted(PurrNet.NetworkManager manager, bool asServer) => Listeners.Dispatch(NotifyOnNetworkStarted, asServer);
        private void HandleNetworkShutdown(PurrNet.NetworkManager manager, bool asServer) => Listeners.Dispatch(NotifyOnNetworkShutdown, asServer);
        private void HandleClientConnectionState(ConnectionState state) => Listeners.Dispatch(NotifyOnClientConnectionState, state);
        private void HandlePlayerJoined(PlayerID id, bool isReconnect, bool asServer) => Listeners.Dispatch(NotifyOnPlayerJoined, id, isReconnect, asServer);
        private void HandlePlayerLeft(PlayerID id, bool asServer) => Listeners.Dispatch(NotifyOnPlayerLeft, id, asServer);

        private static void NotifyOnLobbyCreated(INetworkManagerListener listener, Lobby lobby) => listener.OnLobbyCreated(lobby);
        private static void NotifyOnLobbyEnter(INetworkManagerListener listener, Lobby lobby) => listener.OnLobbyEnter(lobby);
        private static void NotifyOnLobbyStart(INetworkManagerListener listener, Lobby lobby) => listener.OnLobbyStart(lobby);
        private static void NotifyOnLobbyLeave(INetworkManagerListener listener) => listener.OnLobbyLeave();
        private static void NotifyOnNetworkStarted(INetworkManagerListener listener, bool asServer) => listener.OnNetworkStarted(asServer);
        private static void NotifyOnNetworkShutdown(INetworkManagerListener listener, bool asServer) => listener.OnNetworkShutdown(asServer);
        private static void NotifyOnClientConnectionState(INetworkManagerListener listener, ConnectionState state) => listener.OnClientConnectionState(state);
        private static void NotifyOnPlayerJoined(INetworkManagerListener listener, PlayerID id, bool isReconnect, bool asServer) => listener.OnPlayerJoined(id, isReconnect, asServer);
        private static void NotifyOnPlayerLeft(INetworkManagerListener listener, PlayerID id, bool asServer) => listener.OnPlayerLeft(id, asServer);
    }
}