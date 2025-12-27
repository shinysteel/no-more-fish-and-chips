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

namespace FishFlingers.Networking
{
    public interface INetworkManagerListener
    {
        void OnLobbyCreated(SteamLobby lobby);
        void OnLobbyEnter(SteamLobby lobby);
        void OnLobbyLeave();
        void OnLobbyGameServerSet();
        void OnNetworkStarted(bool asServer);
        void OnNetworkShutdown(bool asServer);
        void OnClientConnectionState(ConnectionState state);
        void OnPlayerJoined(PlayerID id, bool isReconnect);
        void OnPlayerLeft(PlayerID id);
    }

    public class NetworkManager : GameSystem<INetworkManagerListener>
    {
        private NetworkManagerConfig _config;
        private PurrNet.NetworkManager _purrnetNetworkManager;
        private SteamLobbyService _steamLobbyService;

        public SteamLobby CurrentLobby => _steamLobbyService.CurrentLobby;

        public UDPTransport Transport => (UDPTransport)_purrnetNetworkManager.transport;
        public PlayerID LocalPlayer => _purrnetNetworkManager.localPlayer;
        public bool IsServer => _purrnetNetworkManager.isServer;

        public override void Initialise(GameManagerConfig gameManagerConfig)
        {
            _config = gameManagerConfig.NetworkManagerConfig;

            _steamLobbyService = new();
            _steamLobbyService.OnLobbyCreated += HandleLobbyCreated;
            _steamLobbyService.OnLobbyEnter += HandleLobbyEnter;
            _steamLobbyService.OnLobbyLeave += HandleLobbyLeave;
            _steamLobbyService.OnLobbyGameServerSet += HandleLobbyGameServerSet;

            _purrnetNetworkManager = Object.Instantiate(_config.PurrnetNetworkManagerPrefab);
            _purrnetNetworkManager.onNetworkStarted += HandleNetworkStarted;
            _purrnetNetworkManager.onNetworkShutdown += HandleNetworkShutdown;
            _purrnetNetworkManager.onClientConnectionState += HandleClientConnectionState;
            _purrnetNetworkManager.onPlayerJoined += HandlePlayerJoined;
            _purrnetNetworkManager.onPlayerLeft += HandlePlayerLeft;

            base.Initialise(gameManagerConfig);
        }

        public override void Shutdown()
        {
            _steamLobbyService.OnLobbyCreated -= HandleLobbyCreated;
            _steamLobbyService.OnLobbyEnter -= HandleLobbyEnter;
            _steamLobbyService.OnLobbyLeave -= HandleLobbyLeave;
            _steamLobbyService.OnLobbyGameServerSet -= HandleLobbyGameServerSet;
            _steamLobbyService.Shutdown();

            _purrnetNetworkManager.onNetworkStarted -= HandleNetworkStarted;
            _purrnetNetworkManager.onNetworkShutdown -= HandleNetworkShutdown;
            _purrnetNetworkManager.onClientConnectionState -= HandleClientConnectionState;
            _purrnetNetworkManager.onPlayerJoined -= HandlePlayerJoined;
            _purrnetNetworkManager.onPlayerLeft -= HandlePlayerLeft;

            base.Shutdown();
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

        public async Task<SteamLobby[]> SearchLobbies()
        {
            SteamLobby[] lobbies = await _steamLobbyService.SearchLobbiesAsync();
            return lobbies;
        }

        public async Task<SteamLobby> CreateLobbyAsync()
        {
            SteamLobby lobby = await _steamLobbyService.CreateLobbyAsync();
            return lobby;
        }

        public async Task<SteamLobby> JoinLobbyAsync(CSteamID lobbyId)
        {
            SteamLobby lobby = await _steamLobbyService.JoinLobbyAsync(lobbyId);
            return lobby;
        }

        public void StartLobby()
        {
            _steamLobbyService.StartLobby();
        }

        public void LeaveLobby()
        {
            _steamLobbyService.LeaveLobby();
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

        private void HandleLobbyCreated(SteamLobby lobby) => Listeners.Dispatch(NotifyOnLobbyCreated, lobby);
        private void HandleLobbyEnter(SteamLobby lobby) => Listeners.Dispatch(NotifyOnLobbyEnter, lobby);
        private void HandleLobbyLeave() => Listeners.Dispatch(NotifyOnLobbyLeave);
        private void HandleLobbyGameServerSet() => Listeners.Dispatch(NotifyOnLobbyGameServerSet);
        private void HandleNetworkStarted(PurrNet.NetworkManager manager, bool asServer) => Listeners.Dispatch(NotifyOnNetworkStarted, asServer);
        private void HandleNetworkShutdown(PurrNet.NetworkManager manager, bool asServer) => Listeners.Dispatch(NotifyOnNetworkShutdown, asServer);
        private void HandleClientConnectionState(ConnectionState state) => Listeners.Dispatch(NotifyOnClientConnectionState, state);
        private void HandlePlayerJoined(PlayerID id, bool isReconnect, bool asServer) => Listeners.Dispatch(NotifyOnPlayerJoined, id, isReconnect, asServer);
        private void HandlePlayerLeft(PlayerID id, bool asServer) => Listeners.Dispatch(NotifyOnPlayerLeft, id, asServer);

        private static void NotifyOnLobbyCreated(INetworkManagerListener listener, SteamLobby lobby) => listener.OnLobbyCreated(lobby);
        private static void NotifyOnLobbyEnter(INetworkManagerListener listener, SteamLobby lobby) => listener.OnLobbyEnter(lobby);
        private static void NotifyOnLobbyLeave(INetworkManagerListener listener) => listener.OnLobbyLeave();
        private static void NotifyOnLobbyGameServerSet(INetworkManagerListener listener) => listener.OnLobbyGameServerSet();
        private static void NotifyOnNetworkStarted(INetworkManagerListener listener, bool asServer) => listener.OnNetworkStarted(asServer);
        private static void NotifyOnNetworkShutdown(INetworkManagerListener listener, bool asServer) => listener.OnNetworkShutdown(asServer);
        private static void NotifyOnClientConnectionState(INetworkManagerListener listener, ConnectionState state) => listener.OnClientConnectionState(state);
        private static void NotifyOnPlayerJoined(INetworkManagerListener listener, PlayerID id, bool isReconnect, bool asServer) => listener.OnPlayerJoined(id, isReconnect);
        private static void NotifyOnPlayerLeft(INetworkManagerListener listener, PlayerID id, bool asServer) => listener.OnPlayerLeft(id);
    }
}