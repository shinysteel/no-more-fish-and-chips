using PurrNet;
using UnityEngine;
using FishFlingers.Entities;
using FishFlingers.Networking;
using PurrNet.Transports;
using FishFlingers.Scenes;
using System.Collections.Generic;
using FishFlingers.States;

namespace FishFlingers.Environments
{
    public class SalvageSpawner : NetBehaviour, INetworkManagerListener
    {
        [SerializeField] private DroppedItem _driftwoodPrefab;

        [SerializeField] private float _spawnInterval = 5f;

        private GameplayContext _context;

        private float _spawnTimer;

        private List<DroppedItem> _salvages = new();

        private const int MaxSalvage = 10;

        protected override void OnSpawned()
        {
            base.OnSpawned();

            _networkManager.AddListener(this);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _networkManager?.RemoveListener(this);
        }

        public void Initialise(GameplayContext context)
        {
            _context = context;
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
            Raft raft = _context.Raft;

            int minSpread = 3;
            float x = Random.Range((float)Mathf.Min(-minSpread, raft.LeftmostColumn), Mathf.Max(minSpread, raft.RightmostColumn));
            int forwardDist = 10;
            int y = raft.ForwardmostRow + forwardDist;
            Vector3 position = raft.CellToWorldPosition(new Vector2(x, y));

            DroppedItem item = _networkManager.Spawn(_driftwoodPrefab, new SpawnParams() { Position = position });
            item.Initialise(_context);

            _salvages.Add(item);
        }

        public void OnNetworkDespawn(NetBehaviour behaviour) 
        {
            if (behaviour is not DroppedItem item)
            {
                return;
            }

            _salvages.Remove(item);
        }

        public void OnNetworkSpawn(NetBehaviour behaviour) { }
        public void OnNetworkStarted(bool asServer) { }
        public void OnNetworkShutdown(bool asServer) { }
        public void OnClientConnectionState(ConnectionState state) { }
        public void OnPlayerJoined(PlayerID id, bool isReconnect, bool asServer) { }
        public void OnPlayerLeft(PlayerID id, bool asServer) { }
    }
}