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
using System.Net.Sockets;
using PurrNet.Steam;
using System.Linq;
using System;
using FishFlingers.Scenes;

using Object = UnityEngine.Object;

namespace FishFlingers.Networking
{
    public enum ETransport
    {
        UDP   ,
        Steam ,
    }

    public interface INetworkManagerListener
    {
        void OnNetworkStarted(bool asServer);
        void OnNetworkShutdown(bool asServer);
        void OnNetworkSpawn(NetBehaviour behaviour);
        void OnNetworkDespawn(NetBehaviour behaviour);
        void OnClientConnectionState(ConnectionState state);
        void OnPlayerJoined(PlayerID id, bool isReconnect, bool asServer);
        void OnPlayerLeft(PlayerID id, bool asServer);
    }

    public class NetworkManager : GameSystem<INetworkManagerListener>, ISceneManagerListener
    {
        private NetworkManagerConfig _config;
        public NetworkManagerConfig Config => _config;

        private SceneManager _sceneManager;

        private PurrNet.NetworkManager _purrnetNetworkManager;

        public PlayerID LocalPlayerId => _purrnetNetworkManager.localPlayer;

        public bool IsServer => _purrnetNetworkManager.isServer;

        public static readonly Vector3 HiddenSpawnPosition = new Vector3(0f, -15f, 0f);

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.NetworkManagerConfig;

            _sceneManager = GameManager.Instance.Get<SceneManager>();
            _sceneManager.AddListener(this);

            _purrnetNetworkManager = Object.Instantiate(_config.PurrnetNetworkManagerPrefab);
            _purrnetNetworkManager.onNetworkStarted += HandleNetworkStarted;
            _purrnetNetworkManager.onNetworkShutdown += HandleNetworkShutdown;
            _purrnetNetworkManager.onClientConnectionState += HandleClientConnectionState;
            _purrnetNetworkManager.onPlayerJoined += HandlePlayerJoined;
            _purrnetNetworkManager.onPlayerLeft += HandlePlayerLeft;

            GetTransport<UDPTransport>().serverPort = _config.UDPServerPort;
            GetTransport<SteamTransport>().serverPort = _config.SteamServerPort;

            // Remove this and let other scripts request
            SetClientTransport<UDPTransport>();

            base.Initialise(config);
        }

        public override void Shutdown()
        {
            _sceneManager?.RemoveListener(this);

            _purrnetNetworkManager.onNetworkStarted -= HandleNetworkStarted;
            _purrnetNetworkManager.onNetworkShutdown -= HandleNetworkShutdown;
            _purrnetNetworkManager.onClientConnectionState -= HandleClientConnectionState;
            _purrnetNetworkManager.onPlayerJoined -= HandlePlayerJoined;
            _purrnetNetworkManager.onPlayerLeft -= HandlePlayerLeft;

            base.Shutdown();
        }

        // We no longer need to raise the OnNetworkSpawn event here, but its nice to route
        // all 'network' spawning here in mind that not all networking solutions let you just instantiate
        public T Spawn<T>(T prefab) where T : NetBehaviour
        {
            return Spawn(prefab, new SpawnParams());
        }

        public T Spawn<T>(T prefab, SpawnParams parameters) where T : NetBehaviour
        {
            T obj = UnityProxy.Instantiate(prefab, parameters.Position, parameters.Rotation, parameters.SpawnScene.Get());
            return obj;
        }

        public void Despawn(NetBehaviour behaviour)
        {
            Object.Destroy(behaviour.gameObject);
        }

        public void RaiseSpawned(NetBehaviour behaviour)
        {
            Listeners.Dispatch(NotifyOnNetworkSpawn, behaviour);
        }

        public void RaiseDespawned(NetBehaviour behaviour)
        {
            Listeners.Dispatch(NotifyOnNetworkDespawn, behaviour);
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

        // Use sparingly, as we want to keep this manager as enclosed as possible
        public T GetModule<T>(bool asServer) where T : INetworkModule
        {
            _purrnetNetworkManager.TryGetModule(asServer, out T module);
            return module;
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

        private void HandleNetworkStarted(PurrNet.NetworkManager manager, bool asServer) => Listeners.Dispatch(NotifyOnNetworkStarted, asServer);
        private void HandleNetworkShutdown(PurrNet.NetworkManager manager, bool asServer) => Listeners.Dispatch(NotifyOnNetworkShutdown, asServer);
        private void HandleClientConnectionState(ConnectionState state) => Listeners.Dispatch(NotifyOnClientConnectionState, state);
        private void HandlePlayerJoined(PlayerID id, bool isReconnect, bool asServer) => Listeners.Dispatch(NotifyOnPlayerJoined, id, isReconnect, asServer);
        private void HandlePlayerLeft(PlayerID id, bool asServer) => Listeners.Dispatch(NotifyOnPlayerLeft, id, asServer);

        private static void NotifyOnNetworkStarted(INetworkManagerListener listener, bool asServer) => listener.OnNetworkStarted(asServer);
        private static void NotifyOnNetworkShutdown(INetworkManagerListener listener, bool asServer) => listener.OnNetworkShutdown(asServer);
        private static void NotifyOnNetworkSpawn(INetworkManagerListener listener, NetBehaviour behaviour) => listener.OnNetworkSpawn(behaviour);
        private static void NotifyOnNetworkDespawn(INetworkManagerListener listener, NetBehaviour behaviour) => listener.OnNetworkDespawn(behaviour);
        private static void NotifyOnClientConnectionState(INetworkManagerListener listener, ConnectionState state) => listener.OnClientConnectionState(state);
        private static void NotifyOnPlayerJoined(INetworkManagerListener listener, PlayerID id, bool isReconnect, bool asServer) => listener.OnPlayerJoined(id, isReconnect, asServer);
        private static void NotifyOnPlayerLeft(INetworkManagerListener listener, PlayerID id, bool asServer) => listener.OnPlayerLeft(id, asServer);

        public void OnPlayerLoadedScene(PlayerID playerId, EScene scene, bool asServer) 
        { 
            if (!asServer)
            {
                return;
            }

            if (GetModule<GlobalOwnershipModule>(true).PlayerOwnsSomething(playerId))
            {
                return;
            }

            PurrnetPlayer player = Spawn(_config.PurrnetPlayerPrefab, new SpawnParams() { SpawnScene = SpawnScene.DontDestroyOnLoad() });
            player.GiveOwnership(playerId);
        }

        public void OnSceneLoaded(EScene scene, LoadSceneMode mode) { }
        public void OnSceneUnloaded(EScene scene) { }
        public void OnNetworkedSceneLoaded(EScene scene, bool asServer) { }
        public void OnNetworkedSceneUnloaded(EScene scene, bool asServer) { }
        public void OnPlayerUnloadedScene(PlayerID playerId, EScene scene, bool asServer) { }
        public void OnActiveSceneChanged(EScene previous, EScene current) { }
    }
}