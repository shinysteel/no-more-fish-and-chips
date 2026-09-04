using NoMoreFishAndChips.Effects;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Instantiating;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Scenes;
using PurrLobby;
using PurrNet;
using PurrNet.Authentication;
using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.Steam;
using PurrNet.Transports;
using ShinyOwl.Common;
using Steam;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

using Object = UnityEngine.Object;

namespace NoMoreFishAndChips.Networking
{
    public enum ETransport
    {
        UDP   ,
        Steam ,
    }

    public interface INetworkManagerListener
    {
        void OnNetworkStarted(bool asServer) { }
        void OnNetworkShutdown(bool asServer) { }
        void OnNetBehaviourSpawned(NetBehaviour behaviour) { }
        void OnNetBehaviourDespawned(NetBehaviour behaviour) { }
        void OnClientConnectionState(ConnectionState state) { }
        void OnPlayerJoined(PlayerID playerId, bool isReconnect, bool asServer) { }
        void OnPlayerLeft(PlayerID playerId, bool asServer) { }
    }

    public class NetworkManager : GameSystem<INetworkManagerListener>, ISceneManagerListener
    {
        private NetworkManagerConfig _config;

        private SceneManager _sceneManager;
        private SteamManager _steamManager;

        private PurrNet.NetworkManager _purrnetNetworkManager;

        public bool IsServer => _purrnetNetworkManager.isServer;

        public IReadOnlyList<PlayerID> PlayerIds => _purrnetNetworkManager.players;
        public PlayerID LocalPlayerId => _purrnetNetworkManager.localPlayer;
        public PlayerID ServerPlayerId => PlayerID.Server;

        private List<PurrnetPlayer> _purrnetPlayers = new();
        public IReadOnlyList<PurrnetPlayer> PurrnetPlayers => _purrnetPlayers;

        private Dictionary<PlayerID, PurrnetPlayer> _playerIdPurrnetPlayerMap = new();
        public PurrnetPlayer LocalPurrnetPlayer => _playerIdPurrnetPlayerMap[LocalPlayerId];

        private const string IsSubscribedClientName = "_isSubscribedClient";
        public bool IsSubscribedClient => (bool)typeof(PurrNet.NetworkManager).GetField(IsSubscribedClientName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_purrnetNetworkManager);

        public float ServerTime => _purrnetNetworkManager.tickModule.TickToTime(_purrnetNetworkManager.tickModule.localTick);

        public static readonly Vector3 HiddenSpawnPosition = new Vector3(0f, -15f, 0f);

        public override void InitialiseConfig(GameManagerConfig config)
        {
            _sceneManager = GameManager.Instance.Get<SceneManager>();
            _steamManager = GameManager.Instance.Get<SteamManager>();

            _sceneManager.AddListener(this);

            _config = config.NetworkManagerConfig;

            _purrnetNetworkManager = Object.Instantiate(_config.PurrnetNetworkManagerPrefab);
            _purrnetNetworkManager.onNetworkStarted += HandleNetworkStarted;
            _purrnetNetworkManager.onNetworkShutdown += HandleNetworkShutdown;
            _purrnetNetworkManager.onClientConnectionState += HandleClientConnectionState;
            _purrnetNetworkManager.onPlayerJoined += HandlePlayerJoined;
            _purrnetNetworkManager.onPlayerLeft += HandlePlayerLeft;

            UDPTransport udpTransport = GetTransport<UDPTransport>();
            SteamTransport steamTransport = GetTransport<SteamTransport>();

            udpTransport.serverPort = _config.UDPServerPort;
            steamTransport.serverPort = _config.SteamServerPort;

            List<GenericTransport> filter = new List<GenericTransport>() { udpTransport };

            if (_steamManager.Initialized)
            {
                filter.Add(steamTransport);
            }

            GetCompositeTransport().SetServerTransportFilter(filter);

            // Remove this and let other scripts request
            SetClientTransport<UDPTransport>();

            // Haven't figured out how to auto-register custom classes that inherit from
            // any SyncCollection class, so at least for now we just have to do that here manually
            PackCollections.RegisterDictionary<Vector2Int, RaftTile>();
            PackCollections.RegisterDictionary<Vector2Int, NetInventorySlot>();
            PackCollections.RegisterDictionary<string, NetInventoryItem>();
            PackCollections.RegisterDictionary<Vector2Int, Structure>();
            PackCollections.RegisterDictionary<int, NetMarker>();
            
            base.InitialiseConfig(config);
        }

        public override void Dispose()
        {
            _sceneManager.RemoveListener(this);

            _purrnetNetworkManager.onNetworkStarted -= HandleNetworkStarted;
            _purrnetNetworkManager.onNetworkShutdown -= HandleNetworkShutdown;
            _purrnetNetworkManager.onClientConnectionState -= HandleClientConnectionState;
            _purrnetNetworkManager.onPlayerJoined -= HandlePlayerJoined;
            _purrnetNetworkManager.onPlayerLeft -= HandlePlayerLeft;

            base.Dispose();
        }

        public PurrnetPlayer GetPurrnetPlayer(PlayerID id)
        {
            return _playerIdPurrnetPlayerMap[id];
        }

        // SyncVar 'onChanged' events aren't fired for the host even when using SetDirty or FlushImmediately. Using reflection
        // we can emulate this
        public void ChangeSyncVar<T>(SyncVar<T> syncVar, Action change) where T : IDeepCloneable<T>
        {
            T oldValue = syncVar.value.DeepClone();
            change?.Invoke();
            syncVar.SetDirty();
            string methodName = "TriggerEvents";
            syncVar.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(syncVar, new object[] { oldValue });
        }

        // We no longer need to raise the OnNetworkSpawn event here, but its nice to route
        // all 'network' spawning here in mind that not all networking solutions let you just instantiate
        public T Spawn<T>(T prefab) where T : NetBehaviour
        {
            return Spawn(prefab, new SpawnParams());
        }

        public T Spawn<T>(T prefab, SpawnParams parameters) where T : NetBehaviour
        {
            return parameters.Parent != null
                ? UnityProxy.Instantiate(prefab, parameters.Position, parameters.Rotation, parameters.Parent)
                : UnityProxy.Instantiate(prefab, parameters.Position, parameters.Rotation, parameters.SpawnScene.Get());
        }

        public void Despawn(NetBehaviour behaviour)
        {
            Object.Destroy(behaviour.gameObject);
        }

        public void RaiseNetBehaviourSpawned(NetBehaviour behaviour)
        {
            if (behaviour is PurrnetPlayer player)
            {
                _purrnetPlayers.Add(player);
                _playerIdPurrnetPlayerMap.Add(behaviour.owner.Value, player);
            }

            NotifyNetBehaviourSpawned(behaviour);
        }

        public void RaiseNetBehaviourDespawned(NetBehaviour behaviour)
        {
            if (behaviour is PurrnetPlayer player)
            {
                _purrnetPlayers.Remove(player);
                _playerIdPurrnetPlayerMap.Remove(behaviour.owner.Value);
            }

            NotifyNetBehaviourDespawned(behaviour);
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

        public void Send<T>(PlayerID playerId, T data, Channel method = Channel.ReliableUnordered)
        {
            _purrnetNetworkManager.Send(playerId, data, method);
        }

        public void Subscribe<T>(PlayerBroadcastDelegate<T> callback, bool asServer) where T : new()
        {
            _purrnetNetworkManager.Subscribe(callback, asServer);
        }

        public void Unsubscribe<T>(PlayerBroadcastDelegate<T> callback) where T : new()
        {
            _purrnetNetworkManager.Unsubscribe(callback);
        }

        public void KickPlayer(PlayerID playerId)
        {
            _purrnetNetworkManager.playerModule.KickPlayer(playerId);
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

        private void HandleNetworkStarted(PurrNet.NetworkManager manager, bool asServer) => NotifyNetworkStarted(asServer);

        private void HandleNetworkShutdown(PurrNet.NetworkManager manager, bool asServer)
        {
            NotifyNetworkShutdown(asServer);

            if (asServer)
            {
                return;
            }

            // Cleanup after a frame to retain the collection during shutdown
            async Task cleanup()
            {
                await Task.Yield();
                _playerIdPurrnetPlayerMap.Clear();
            }

            _ = cleanup();
        }

        private void HandleClientConnectionState(ConnectionState state) => NotifyClientConnectionState(state);
        private void HandlePlayerJoined(PlayerID playerId, bool isReconnect, bool asServer) => NotifyPlayerJoined(playerId, isReconnect, asServer);
        private void HandlePlayerLeft(PlayerID playerId, bool asServer) => NotifyPlayerLeft(playerId, asServer);
        
        private void NotifyNetworkStarted(bool asServer) => Listeners.Dispatch(listener => listener.OnNetworkStarted(asServer));
        private void NotifyNetworkShutdown(bool asServer) => Listeners.Dispatch(listener => listener.OnNetworkShutdown(asServer));
        private void NotifyNetBehaviourSpawned(NetBehaviour behaviour) => Listeners.Dispatch(listener => listener.OnNetBehaviourSpawned(behaviour));
        private void NotifyNetBehaviourDespawned(NetBehaviour behaviour) => Listeners.Dispatch(listener => listener.OnNetBehaviourDespawned(behaviour));
        private void NotifyClientConnectionState(ConnectionState state) => Listeners.Dispatch(listener => listener.OnClientConnectionState(state));
        private void NotifyPlayerJoined(PlayerID playerId, bool isReconnect, bool asServer) => Listeners.Dispatch(listener => listener.OnPlayerJoined(playerId, isReconnect, asServer));
        private void NotifyPlayerLeft(PlayerID playerId, bool asServer) => Listeners.Dispatch(listener => listener.OnPlayerLeft(playerId, asServer));

        void ISceneManagerListener.OnPlayerLoadedScene(PlayerID playerId, EScene scene, bool asServer) 
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
    }
}