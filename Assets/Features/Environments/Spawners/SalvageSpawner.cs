using PurrNet;
using UnityEngine;
using FishFlingers.Entities;
using FishFlingers.Networking;
using PurrNet.Transports;
using FishFlingers.Scenes;
using System.Collections.Generic;
using FishFlingers.States;
using FishFlingers.Items;

using EntityId = FishFlingers.Entities.EntityId;

namespace FishFlingers.Environments
{
    public class SalvageSpawner : GameplayBehaviour, INetworkManagerListener
    {
        [SerializeField] private float _spawnInterval = 5f;

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

            DroppedItem item = (DroppedItem)_entityManager.Spawn(EntityId.DroppedItem, new SpawnParams() { Position = position });
            item.SetItem(null, ItemId.Driftwood, 1);

            _salvages.Add(item);
        }

        void INetworkManagerListener.OnNetBehaviourDespawned(NetBehaviour behaviour) 
        {
            if (behaviour is not DroppedItem item)
            {
                return;
            }

            _salvages.Remove(item);
        }
    }
}