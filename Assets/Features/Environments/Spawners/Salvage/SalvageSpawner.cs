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

namespace NoMoreFishAndChips.Environments
{
    public class SalvageSpawner : GameplayBehaviour, IEntityManagerListener, IStateManagerListener
    {
        [SerializeField] private float _spawnInterval = 5f;
        [SerializeField] private DropTable _dropTable;

        private bool _isSpawning;
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
            ((IStateManagerListener)this).OnStatePathChanged(null, _stateManager.CurrentStatePath);

            _entityManager.AddListener(this);
            _stateManager.AddListener(this);

            base.OnSpawned();
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _entityManager.RemoveListener(this);
            _stateManager.RemoveListener(this);
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

            if (!_isSpawning)
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
            if (!_context.Raft.Queries.Axes[Axis.Horizontal].TryGetLinesBounds(out IntRange horizontalBounds)
                || !_context.Raft.Queries.Axes[Axis.Vertical].TryGetLinesBounds(out IntRange verticalBounds))
            {
                return;
            }

            int minSpread = 3;

            float x = Random.Range((float)Mathf.Min(-minSpread, horizontalBounds.Min), Mathf.Max(minSpread, horizontalBounds.Max));

            int forwardDist = 10;

            int y = verticalBounds.Max + forwardDist;

            Vector3 position = _context.Raft.Queries.CellToWorldPosition(new Vector2(x, y));

            _itemManager.SpawnDrops(position, DroppedItemType.Salvage, _dropTable);
        }

        void IEntityManagerListener.OnEntitySpawned(Entity entity)
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

        void IEntityManagerListener.OnEntityDespawned(Entity entity)
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

        void IStateManagerListener.OnStatePathChanged(StatePath previous, StatePath current)
        {
            _isSpawning = current.Contains(EGameplayState.Stage);

            if (!_isSpawning)
            {
                _spawnTimer = 0f;
            }
        }
    }
}