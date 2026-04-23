using FishFlingers.Networking;
using UnityEngine;
using PurrNet;
using System.Collections.Generic;
using FishFlingers.Entities;
using ShinyOwl.Common;

namespace FishFlingers.Effects
{
    public enum TileMarkShape
    {
        Single,
        OneByTwo
    }

    public class NetTileMark
    {
        private Vector2Int _cell;
        private TileMarkShape _shape;

        public Vector2Int Cell => _cell;
        public TileMarkShape Shape => _shape;

        public NetTileMark(Vector2Int cell, TileMarkShape shape)
        {
            _cell = cell;
            _shape = shape;
        }
    }

    public class TileMarker : GameplayBehaviour
    {
        private SyncDictionaryWrapper<int, NetTileMark> _netTileMarks = new SyncDictionaryWrapper<int, NetTileMark>(ownerAuth: true);

        private Dictionary<int, TileMark> _tileMarks = new();

        private int _idCounter;

        protected override void OnSpawned()
        {
            foreach (KeyValuePair<int, NetTileMark> kvp in _netTileMarks)
            {
                SyncDictionaryChange<int, NetTileMark> change = new SyncDictionaryChange<int, NetTileMark>(SyncDictionaryOperation.Added, kvp.Key, kvp.Value);
                HandleNetMarkedCellsChanged(change);
            }

            _netTileMarks.onChanged += HandleNetMarkedCellsChanged;

            base.OnSpawned();
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _netTileMarks.onChanged -= HandleNetMarkedCellsChanged;
        }

        public int AddNetMarkedCell(NetTileMark mark)
        {
            int id = _idCounter++;
            _netTileMarks.Add(id, mark);

            return id;
        }

        public void RemoveNetMarkedCell(int id)
        {
            _netTileMarks.Remove(id);
        }

        private void HandleNetMarkedCellsChanged(SyncDictionaryChange<int, NetTileMark> change)
        {
            if (change.operation == SyncDictionaryOperation.Added)
            {
                Tile tile = _context.Raft.Tiles[change.value.Cell];
                TileMark mark = _poolManager.GetPoolable<TileMark>(new SpawnParams() { Parent = tile.transform });
                mark.Initialise(_context, tile);
                _tileMarks.Add(change.key, mark);
            }
            else if (change.operation == SyncDictionaryOperation.Removed)
            {
                _poolManager.ReturnPoolable(_tileMarks[change.key]);
                _tileMarks.Remove(change.key);
            }
        }
    }
}