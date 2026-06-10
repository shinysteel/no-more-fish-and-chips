using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Scenes;
using NoMoreFishAndChips.States;
using PurrNet;
using PurrNet.Transports;
using ShinyOwl.Common;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Utils;
using EntityId = NoMoreFishAndChips.Entities.EntityId;

namespace NoMoreFishAndChips.Environments
{
    public class SalvageSpawner : GameplayBehaviour, IEntityManagerListener
    {
        [SerializeField] private float _spawnInterval = 5f;
        [SerializeField] private DropTable _dropTable;

        private float _spawnTimer;
        private WeightedPicker<ItemId> _weightedPicker = new();

        private List<DroppedItem> _salvages = new();

        private const int MaxSalvage = 10;

        protected override void Awake()
        {
            base.Awake();

            _weightedPicker.Set(_dropTable.Entries);
        }

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

            float x = Random.Range(
                (float)Mathf.Min(-minSpread, _context.Raft.Queries.Axes[Axis.Horizontal].MinLineIndex), 
                Mathf.Max(minSpread, _context.Raft.Queries.Axes[Axis.Horizontal].MaxLineIndex));

            int forwardDist = 10;

            int y = _context.Raft.Queries.Axes[Axis.Vertical].MaxLineIndex + forwardDist;

            Vector3 position = _context.Raft.Queries.CellToWorldPosition(new Vector2(x, y));

            _itemManager.SpawnDrops(position, DroppedItemType.Salvage, _dropTable);
        }

        void IEntityManagerListener.OnEntitySpawned(IEntity entity)
        {
            if (entity is not DroppedItem item)
            {
                return;
            }

            if (item.Type != DroppedItemType.Salvage)
            {
                return;
            }

            _salvages.Add(item);
        }

        void IEntityManagerListener.OnEntityDespawned(IEntity entity)
        {
            if (entity is not DroppedItem item)
            {
                return;
            }

            if (item.Type != DroppedItemType.Salvage)
            {
                return;
            }

            _salvages.Remove(item);
        }
    }
}