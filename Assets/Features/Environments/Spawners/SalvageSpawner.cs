using PurrNet;
using UnityEngine;
using FishFlingers.Entities;
using FishFlingers.Networking;
using PurrNet.Transports;
using FishFlingers.Scenes;
using System.Collections.Generic;

using NetworkManager = FishFlingers.Networking.NetworkManager;

namespace FishFlingers.Environments
{
    public class SalvageSpawner : NetworkBehaviour, INetworkManagerListener
    {
        [SerializeField] private DroppedItem _driftwoodPrefab;

        [SerializeField] private float _spawnInterval = 5f;

        private NetworkManager _networkManager;

        private Raft _raft;

        private float _spawnTimer;

        private List<DroppedItem> _salvages = new();

        private const int MaxSalvage = 10;

        public void Initialise(Raft raft)   
        {
            _raft = raft;
        }

        protected override void OnInitializeModules()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
        }

        protected override void OnSpawned()
        {
            _networkManager.AddListener(this);
        }

        protected override void OnDespawned()
        {
            _networkManager?.RemoveListener(this);
        }

        private void Update()
        {
            SpawnUpdate();
        }

        private void SpawnUpdate()
        {
            if (!isServer)
            {
                return;
            }

            if (_spawnTimer < _spawnInterval)
            {
                _spawnTimer += Time.deltaTime;
                return;
            }

            if (_salvages.Count >= MaxSalvage)
            {
                return;
            }

            _spawnTimer -= _spawnInterval;

            Spawn();
        }

        private void Spawn()
        {
            int minSpread = 3;
            float x = Random.Range((float)Mathf.Min(-minSpread, _raft.LeftmostColumn), Mathf.Max(minSpread, _raft.RightmostColumn));
            int forwardDist = 10;
            int y = _raft.ForwardmostRow + forwardDist;
            Vector3 position = _raft.CellToWorldPosition(new Vector2(x, y));

            DroppedItem item = _networkManager.Spawn(_driftwoodPrefab, position);
            item.Initialise(_raft);

            _salvages.Add(item);
        }

        public void OnNetworkDespawn() 
        {
            _salvages.RemoveAll(salvage => salvage == null);
        }

        public void OnNetworkSpawn() { }
        public void OnLobbyCreated(Lobby lobby) { }
        public void OnLobbyEnter(Lobby lobby) { }
        public void OnLobbyStart(Lobby lobby) { }
        public void OnLobbyLeave() { }
        public void OnNetworkStarted(bool asServer) { }
        public void OnNetworkShutdown(bool asServer) { }
        public void OnClientConnectionState(ConnectionState state) { }
        public void OnPlayerJoined(PlayerID id, bool isReconnect, bool asServer) { }
        public void OnPlayerLeft(PlayerID id, bool asServer) { }
        public void OnNetworkSceneLoaded(EScene scene, bool asServer) { }
        public void OnNetworkSceneUnloaded(EScene scene, bool asServer) { }
    }
}