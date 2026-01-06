using System.Timers;
using UnityEngine;
using System.Collections.Generic;
using ShinyOwl.Common;
using PurrNet;
using FishFlingers.Pools;
using System.Linq;
using FishFlingers.Entities;

namespace FishFlingers.Environments
{
    public partial class Raft : NetworkBehaviour
    {
        [SerializeField] private Transform _tilesContainer;

        private PoolManager _poolManager;

        private SyncDictionary<Vector2Int, NetTile> _netTiles = new();

        private Dictionary<Vector2Int, Tile> _tiles = new();

        // Every column will have x rows, and every row will have x columns
        private Dictionary<int, SortedSet<int>> _columnToRowsMap = new();
        private Dictionary<int, SortedSet<int>> _rowToColumnsMap = new();

        private int _forwardmostRow;
        private int _backmostRow;
        private int _rightmostColumn;
        private int _leftmostColumn;

        public int ForwardmostRow => _forwardmostRow;
        public int BackmostRow => _backmostRow;
        public int RightmostColumn => _rightmostColumn;
        public int LeftmostColumn => _leftmostColumn;

        public class NetTile
        {
            public int Health { get; private set; }

            public const int MaxHealth = 3;

            public NetTile(int health)
            {
                SetHealth(health);
            }

            public void SetHealth(int health)
            {
                Health = Mathf.Clamp(health, 0, MaxHealth);
            }
        }

        protected override void OnInitializeModules()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _netTiles.onChanged += HandleNetTilesChanged;
        }

        protected override void OnSpawned()
        {
            if (isOwner)
            {
                // Start with a 3x3 grid
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        _netTiles.Add(new Vector2Int(x, y), new NetTile(NetTile.MaxHealth));
                    }
                }
            }
            else
            {
                // We need to manually handles changes that have happened before we joined
                foreach (KeyValuePair<Vector2Int, NetTile> kvp in _netTiles)
                {
                    SyncDictionaryChange<Vector2Int, NetTile> change = new(SyncDictionaryOperation.Set, kvp.Key, kvp.Value);
                    HandleNetTilesChanged(change);
                }
            }
        }

        // No tile param is good here, since it lets callers request to damage a cell without having to worry
        // if it exists anymore
        public void ChangeNetTileHealth(Vector2Int cell, int change)
        {
            if (!isOwner)
            {
                return;
            }

            if (!_netTiles.TryGetValue(cell, out NetTile netTile))
            {
                return;
            }

            netTile.SetHealth(netTile.Health + change);

            if (netTile.Health > 0)
            {
                _netTiles.SetDirty(cell);
            }
            else
            {
                _netTiles.Remove(cell);
            }
        }

        private void HandleNetTilesChanged(SyncDictionaryChange<Vector2Int, NetTile> change)
        {
            // Tile no longer exists
            if (change.value == null || change.value.Health <= 0)
            {
                RemoveTile(change.key);
            }
            // Tile exists
            else
            {
                SetTile(change.key, change.value);
            }
        }

        private void RemoveTile(Vector2Int cell)
        {
            // Return to pool
            if (_tiles.TryGetValue(cell, out Tile tile))
            {
                _poolManager.Return(tile);
            }

            _tiles.Remove(tile.Cell);

            RemoveTileUpdateMaps(cell);
            RemoveTileUpdateBoundaries(cell);
        }

        // Adds a new tile, or updates an existing one
        private void SetTile(Vector2Int cell, NetTile netTile)
        {
            // Retrieve from pool
            if (!_tiles.ContainsKey(cell))
            {
                _tiles[cell] = _poolManager.Get<Tile>(_tilesContainer);
                _tiles[cell].Initialise(this);
            }

            Tile tile = _tiles[cell];

            tile.SetCell(new Vector2Int(cell.x, cell.y));
            tile.SetHealth(netTile.Health);

            SetTileUpdateMaps(cell);
            SetTileUpdateBoundaries(cell);
        }

        private void RemoveTileUpdateMaps(Vector2Int cell)
        {
            _columnToRowsMap[cell.x].Remove(cell.y);
            _rowToColumnsMap[cell.y].Remove(cell.x);

            if (_columnToRowsMap[cell.x].Count == 0)
            {
                _columnToRowsMap.Remove(cell.x);
            }

            if (_rowToColumnsMap[cell.y].Count == 0)
            {
                _rowToColumnsMap.Remove(cell.y);
            }
        }

        private void SetTileUpdateMaps(Vector2Int cell)
        {
            if (!_columnToRowsMap.ContainsKey(cell.x))
            {
                _columnToRowsMap.Add(cell.x, new());
            }

            if (!_rowToColumnsMap.ContainsKey(cell.y))
            {
                _rowToColumnsMap.Add(cell.y, new());
            }

            _columnToRowsMap[cell.x].Add(cell.y);
            _rowToColumnsMap[cell.y].Add(cell.x);
        }

        private void RemoveTileUpdateBoundaries(Vector2Int cell)
        {
            if (_forwardmostRow == cell.y && !_rowToColumnsMap.ContainsKey(cell.y))
            {
                _forwardmostRow = _rowToColumnsMap.Count > 0 ? _rowToColumnsMap.Keys.Max() : 0;
            }

            if (_backmostRow == cell.y && !_rowToColumnsMap.ContainsKey(cell.y))
            {
                _backmostRow = _rowToColumnsMap.Count > 0 ? _rowToColumnsMap.Keys.Min() : 0;
            }

            if (_rightmostColumn == cell.x && !_columnToRowsMap.ContainsKey(cell.x))
            {
                _rightmostColumn = _columnToRowsMap.Count > 0 ? _columnToRowsMap.Keys.Max() : 0;
            }

            if (_leftmostColumn == cell.x && !_columnToRowsMap.ContainsKey(cell.x))
            {
                _leftmostColumn = _columnToRowsMap.Count > 0 ? _columnToRowsMap.Keys.Min() : 0;
            }
        }

        private void SetTileUpdateBoundaries(Vector2Int cell)
        {
            _forwardmostRow = Mathf.Max(_forwardmostRow, cell.y);
            _backmostRow = Mathf.Min(_backmostRow, cell.y);
            _rightmostColumn = Mathf.Max(_rightmostColumn, cell.x);
            _leftmostColumn = Mathf.Min(_leftmostColumn, cell.x);
        }
    }
}