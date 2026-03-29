using FishFlingers.Entities;
using FishFlingers.Items;
using FishFlingers.Networking;
using FishFlingers.Scenes;
using FishFlingers.States;
using PurrNet;
using PurrNet.Transports;
using System.Collections.Generic;
using UnityEngine;
using EntityId = FishFlingers.Entities.EntityId;

namespace FishFlingers.Environments
{
    public class SalvageSpawner : GameplayBehaviour, IEntityManagerListener
    {
        [SerializeField] private float _spawnInterval = 5f;

        private float _spawnTimer;

        private List<DroppedItem> _salvages = new();

        private const int MaxSalvage = 10;

        protected override void OnSpawned()
        {
            base.OnSpawned();

            _entityManager.AddListener(this);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _entityManager?.RemoveListener(this);
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
            float x = Random.Range((float)Mathf.Min(-minSpread, _context.Raft.LeftmostColumn), Mathf.Max(minSpread, _context.Raft.RightmostColumn));
            int forwardDist = 10;
            int y = _context.Raft.ForwardmostRow + forwardDist;
            Vector3 position = _context.Raft.CellToWorldPosition(new Vector2(x, y));

            DroppedItem item = (DroppedItem)_entityManager.Spawn(EntityId.DroppedItem, new SpawnParams() { Position = position });
            item.SetNetItemInstance(new NetItemInstance(null, ItemId.Driftwood, 1));

            _salvages.Add(item);
        }

        void IEntityManagerListener.OnEntityDespawned(IEntity entity)
        {
            if (entity is not DroppedItem item)
            {
                return;
            }

            _salvages.Remove(item);
        }
    }
}