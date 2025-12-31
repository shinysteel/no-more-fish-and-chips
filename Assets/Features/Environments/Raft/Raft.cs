using System.Timers;
using UnityEngine;
using System.Collections.Generic;
using ShinyOwl.Common;
using PurrNet;
using FishFlingers.Pools;

namespace FishFlingers.Environments
{
    public class Raft : NetworkBehaviour
    {
        public class NetTile
        {
            public int Health { get; private set; }

            public NetTile(int health)
            {
                Health = health;
            }
        }

        [SerializeField] private Transform _tilesContainer;

        private PoolManager _poolManager;

        private SyncDictionary<Vector2Int, NetTile> _netTiles = new();

        private Dictionary<Vector2Int, Tile> _tiles = new();

        protected override void OnInitializeModules()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _netTiles.onChanged += HandleNetTilesChanged;

            // Start with a 3x3 grid
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    _netTiles.Add(new Vector2Int(x, y), new NetTile(Tile.DefaultHealth));
                }
            }
        }

        protected override void OnSpawned()
        {
            if (!isOwner)
            {
                // We need to manually handles changes that have happened before we joined
                foreach (KeyValuePair<Vector2Int, NetTile> kvp in _netTiles)
                {
                    SyncDictionaryChange<Vector2Int, NetTile> change = new(SyncDictionaryOperation.Set, kvp.Key, kvp.Value);
                    HandleNetTilesChanged(change);
                }

                return;
            }
        }

        private void HandleNetTilesChanged(SyncDictionaryChange<Vector2Int, NetTile> change)
        {
            _tiles.TryGetValue(change.key, out Tile tile);

            // Tile no longer exists
            if (change.value == null || change.value.Health <= 0)
            {
                if (tile != null)
                {
                    _poolManager.Return(tile);
                    _tiles.Remove(change.key);
                }

                return;
            }

            // Tile exists
            if (tile == null)
            {
                tile = _poolManager.Get<Tile>(_tilesContainer);
            }

            // Update the tile's values using change.value
            tile.SetCell(new Vector2Int(change.key.x, change.key.y));
            tile.SetHealth(change.value.Health);

            _tiles[change.key] = tile;
        }
    }
}