using FishFlingers.Entities;
using FishFlingers.Networking;
using FishFlingers.Pools;
using FishFlingers.Saving;
using FishFlingers.Scenes;
using FishFlingers.States;
using Newtonsoft.Json;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using EntityId = FishFlingers.Entities.EntityId;
using Random = UnityEngine.Random;

namespace FishFlingers.Environments
{
    public class RaftSave
    {
        [JsonProperty] public List<TileSave> Tiles { get; private set; } = new();
        [JsonProperty] public List<StructureSave> Structures { get; private set; } = new();

        public void ApplyDefaults()
        {
            // Start with a 3x3 grid
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int health = NetTile.MaxHealth;

                    // 33% chance to have one less health
                    if (Random.value < 1f / 3f)
                    {
                        health--;
                    }

                    Tiles.Add(new TileSave(new Vector2Int(x, y), health));
                }
            }

            // Start with a wave sign
            Structures.Add(new StructureSave(new Vector2Int(0, 1), EntityId.WaveSign));
        }
    }

    public class NetTile
    {
        public int Health { get; private set; }

        public const int MaxHealth = 3;

        public Structure Structure { get; private set; }

        public NetTile(int health)
        {
            SetHealth(health);
        }

        public void SetHealth(int health)
        {
            Health = Mathf.Clamp(health, 0, MaxHealth);
        }

        public void SetStructure(Structure structure)
        {
            Structure = structure;
        }
    }

    public partial class Raft : GameplayBehaviour, ISaveable
    {
        [SerializeField] private Transform _tilesContainer;

        private SyncDictionaryWrapper<Vector2Int, NetTile> _netTiles = new SyncDictionaryWrapper<Vector2Int, NetTile>(ownerAuth: true);

        private Dictionary<Vector2Int, RaftTile> _raftTiles = new();
        public IReadOnlyDictionary<Vector2Int, RaftTile> RaftTiles => _raftTiles;

        public event Action<Vector2Int, RaftTile> OnRaftTileChanged;

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

        public override void Initialise(GameplayContext context)
        {
            base.Initialise(context);

            _instantiateManager.RaiseComponentInstantiated(this);

            _netTiles.onChanged += HandleNetTilesChanged;

            if (isOwner)
            {
                return;
            }

            // Clients need to manually handle changes that have happened before we joined
            foreach (KeyValuePair<Vector2Int, NetTile> kvp in _netTiles)
            {
                SyncDictionaryChange<Vector2Int, NetTile> change = new(SyncDictionaryOperation.Added, kvp.Key, kvp.Value);
                HandleNetTilesChanged(change);
            }
        }

        protected override void OnDespawned()
        {
            _instantiateManager.RaiseComponentDestroyed(this);
        }

        [ServerRpc(requireOwnership: false)]
        public void AddNetTileRpc(Vector2Int cell, int health)
        {
            _netTiles.TryAdd(cell, new NetTile(health));
        }

        [ServerRpc(requireOwnership: false)]
        public void AddStructureRpc(Vector2Int cell, EntityId structureId)
        {
            AddStructure(cell, structureId);
        }

        // No tile param is good here, since it lets callers request to damage a cell without having to worry
        // if it exists anymore
        public void ChangeNetTileHealth(Vector2Int cell, int change)
        {
            if (!isOwner)
            {
                return;
            }

            if (change == 0)
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

        private void AddStructure(Vector2Int cell, EntityId structureId)
        {
            if (!isOwner)
            {
                return;
            }

            if (_entityManager.GetEntity(structureId) is not Structure)
            {
                return;
            }

            if (!_netTiles.TryGetValue(cell, out NetTile netTile))
            {
                return;
            }

            if (netTile.Structure != null)
            {
                return;
            }

            if (!_raftTiles.TryGetValue(cell, out RaftTile raftTile))
            {
                return;
            }

            Structure structure = (Structure)_entityManager.Spawn(structureId, new SpawnParams() { Parent = raftTile.transform, Position = new Vector3(raftTile.transform.position.x, raftTile.GetSurfaceY(), raftTile.transform.position.z) });
            netTile.SetStructure(structure);

            _netTiles.SetDirty(cell);
        }

        private void HandleNetTilesChanged(SyncDictionaryChange<Vector2Int, NetTile> change)
        {
            // Tile exists
            if (change.operation != SyncDictionaryOperation.Removed && change.value != null)
            {
                SetRaftTile(change.key, change.value);
            }
            // Tile no longer exists
            else
            {
                RemoveRaftTile(change.key);
            }
        }

        // Adds a new tile, or updates an existing one
        private void SetRaftTile(Vector2Int cell, NetTile netTile)
        {
            // Retrieve from pool
            if (!_raftTiles.ContainsKey(cell))
            {
                _raftTiles[cell] = (RaftTile)_entityManager.Spawn(EntityId.RaftTile, new SpawnParams() { Parent = _tilesContainer });
                _raftTiles[cell].Initialise(_context);
            }

            RaftTile raftTile = _raftTiles[cell];

            raftTile.SetHealth(netTile.Health);
            raftTile.SetCell(cell);
            raftTile.SetStructure(netTile.Structure);

            SetRaftTileUpdateMaps(cell);
            SetRaftTileUpdateBoundaries(cell);

            OnRaftTileChanged?.Invoke(cell, raftTile);
        }

        private void RemoveRaftTile(Vector2Int cell)
        {
            if (!_raftTiles.TryGetValue(cell, out RaftTile raftTile))
            {
                return;
            }

            // Return to pool
            _entityManager.Despawn(raftTile);

            _raftTiles.Remove(raftTile.Cell);

            RemoveRaftTileUpdateMaps(cell);
            RemoveRaftTileUpdateBoundaries(cell);

            OnRaftTileChanged?.Invoke(cell, null);
        }

        // Maintains positional maps when SetRaftTile is called
        private void SetRaftTileUpdateMaps(Vector2Int cell)
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

        private void RemoveRaftTileUpdateMaps(Vector2Int cell)
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

        // Recalculates boundaries when SetRaftTile is called
        private void SetRaftTileUpdateBoundaries(Vector2Int cell)
        {
            _forwardmostRow = Mathf.Max(_forwardmostRow, cell.y);
            _backmostRow = Mathf.Min(_backmostRow, cell.y);
            _rightmostColumn = Mathf.Max(_rightmostColumn, cell.x);
            _leftmostColumn = Mathf.Min(_leftmostColumn, cell.x);
        }

        private void RemoveRaftTileUpdateBoundaries(Vector2Int cell)
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

        async Task ISaveable.LoadAsync()
        {
            foreach (TileSave save in _saveManager.GameSave.Raft.Tiles)
            {
                AddNetTileRpc(save.Cell.ToVector2Int(), save.Health);
            }

            foreach (StructureSave save in _saveManager.GameSave.Raft.Structures)
            {
                AddStructureRpc(save.Cell.ToVector2Int(), save.StructureId);
            }
        }

        void ISaveable.Save()
        {
            _saveManager.GameSave.Raft.Tiles.Clear();

            foreach (RaftTile tile in _raftTiles.Values)
            {
                _saveManager.GameSave.Raft.Tiles.Add(new TileSave(tile.Cell, tile.CurrentHealth));
            }

            _saveManager.GameSave.Raft.Structures.Clear();

            foreach (RaftTile tile in _raftTiles.Values)
            {
                if (tile.Structure != null)
                {
                    _saveManager.GameSave.Raft.Structures.Add(new StructureSave(tile.Cell, tile.Structure.StructureData.Id));
                }
            }
        }
    }
}