using PurrLobby;
using PurrNet;
using PurrNet.Modules;
using ShinyOwl.Common;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FishFlingers.Networking
{
    public interface INetworkManagerListener
    {
        void OnLobbyCreated(SteamLobby lobby);
        void OnLobbyEnter(SteamLobby lobby);
        void OnLobbyLeave();
        void OnLobbyGameServerSet();
        void OnPlayerJoined(PlayerID id, bool isReconnect);
        void OnPlayerLeft(PlayerID id);
        void OnNetworkShutdown(bool asServer);
    }

    public class NetworkManager : GameSystem<INetworkManagerListener>
    {
        private NetworkManagerConfig _config;
        private PurrNet.NetworkManager _purrnetNetworkManager;
        private SteamLobbyService _steamLobbyService;

        public SteamLobby CurrentLobby => _steamLobbyService.CurrentLobby;
        public bool IsServer => _purrnetNetworkManager.isServer;

        public override void Initialise(GameManagerConfig gameManagerConfig)
        {
            _config = gameManagerConfig.NetworkManagerConfig;

            _purrnetNetworkManager = Object.Instantiate(_config.PurrnetNetworkManagerPrefab);
            _purrnetNetworkManager.onPlayerJoined += HandlePlayerJoined;
            _purrnetNetworkManager.onPlayerLeft += HandlePlayerLeft;
            _purrnetNetworkManager.onNetworkShutdown += HandleNetworkShutdown;

            _steamLobbyService = new();
            _steamLobbyService.OnLobbyCreated += HandleLobbyCreated;
            _steamLobbyService.OnLobbyEnter += HandleLobbyEnter;
            _steamLobbyService.OnLobbyLeave += HandleLobbyLeave;
            _steamLobbyService.OnLobbyGameServerSet += HandleLobbyGameServerSet;

            base.Initialise(gameManagerConfig);
        }

        public override void Shutdown()
        {
            _purrnetNetworkManager.onPlayerJoined -= HandlePlayerJoined;
            _purrnetNetworkManager.onPlayerLeft -= HandlePlayerLeft;
            _purrnetNetworkManager.onNetworkShutdown -= HandleNetworkShutdown;

            _steamLobbyService.OnLobbyCreated -= HandleLobbyCreated;
            _steamLobbyService.OnLobbyEnter -= HandleLobbyEnter;
            _steamLobbyService.OnLobbyLeave -= HandleLobbyLeave;
            _steamLobbyService.OnLobbyGameServerSet -= HandleLobbyGameServerSet;
            _steamLobbyService.Shutdown();

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
        private void HandlePlayerJoined(PlayerID id, bool isReconnect, bool asServer) => Listeners.Dispatch(NotifyOnPlayerJoined, id, isReconnect, asServer);
        private void HandlePlayerLeft(PlayerID id, bool asServer) => Listeners.Dispatch(NotifyOnPlayerLeft, id, asServer);
        private void HandleNetworkShutdown(PurrNet.NetworkManager manager, bool asServer) => Listeners.Dispatch(NotifyOnNetworkShutdown, asServer);

        private static void NotifyOnLobbyCreated(INetworkManagerListener listener, SteamLobby lobby) => listener.OnLobbyCreated(lobby);
        private static void NotifyOnLobbyEnter(INetworkManagerListener listener, SteamLobby lobby) => listener.OnLobbyEnter(lobby);
        private static void NotifyOnLobbyLeave(INetworkManagerListener listener) => listener.OnLobbyLeave();
        private static void NotifyOnLobbyGameServerSet(INetworkManagerListener listener) => listener.OnLobbyGameServerSet();
        private static void NotifyOnPlayerJoined(INetworkManagerListener listener, PlayerID id, bool isReconnect, bool asServer) => listener.OnPlayerJoined(id, isReconnect);
        private static void NotifyOnPlayerLeft(INetworkManagerListener listener, PlayerID id, bool asServer) => listener.OnPlayerLeft(id);
        private static void NotifyOnNetworkShutdown(INetworkManagerListener listener, bool asServer) => listener.OnNetworkShutdown(asServer);
    }
}