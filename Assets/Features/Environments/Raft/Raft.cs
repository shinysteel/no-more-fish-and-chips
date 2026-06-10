using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.Saving;
using NoMoreFishAndChips.Scenes;
using NoMoreFishAndChips.States;
using Newtonsoft.Json;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using EntityId = NoMoreFishAndChips.Entities.EntityId;

namespace NoMoreFishAndChips.Environments
{
    public class NetTile
    {
        public EntityId TileId { get; private set; }
        public int Health { get; private set; }

        public int Rotations { get; private set; }
        public Structure Structure { get; private set; }

        public NetTile(EntityId tileId, int health, int rotations)
        {
            TileId = tileId;
            SetHealth(health);
            Rotations = rotations;
        }

        public void SetHealth(int health)
        {
            Health = health;
        }

        public void SetStructure(Structure structure)
        {
            Structure = structure;
        }
    }

    public class Raft : GameplayBehaviour
    {
        [SerializeField] private Transform _tilesContainer;

        private SyncDictionaryWrapper<Vector2Int, NetTile> _netTiles = new SyncDictionaryWrapper<Vector2Int, NetTile>(ownerAuth: true);

        private Dictionary<Vector2Int, Tile> _tiles = new();
        public IReadOnlyDictionary<Vector2Int, Tile> Tiles => _tiles;

        public event Action<Vector2Int,  Tile> OnTileChanged;

        private RaftQueries _queries;
        public RaftQueries Queries => _queries;

        public override void Initialise(GameplayContext context)
        {
            base.Initialise(context);

            _instantiateManager.RaiseComponentInstantiated(this);

            _netTiles.onChanged += HandleNetTilesChanged;

            _queries = new RaftQueries(this);

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

            foreach (Tile tile in _tiles.Values)
            {
                if (tile.Structure != null)
                {
                    tile.Structure.transform.SetParent(null);
                }
                
                _entityManager.Despawn(tile);
            }
        }

        [ServerRpc(requireOwnership: false)]
        public void AddNetTileRpc(Vector2Int cell, EntityId tileId, int health, int rotations)
        {
            _netTiles.TryAdd(cell, new NetTile(tileId, health, rotations));
        }

        [ServerRpc(requireOwnership: false)]
        public void AddStructureRpc(Vector2Int cell, EntityId structureId)
        {
            AddStructure(cell, structureId);
        }

        // No tile param is good here, since it lets callers request to damage a cell without having to worry
        // if it exists anymore
        public void SetNetTileHealth(Vector2Int cell, int health)
        {
            if (!isOwner)
            {
                return;
            }

            if (!_netTiles.TryGetValue(cell, out NetTile netTile))
            {
                return;
            }

            if (netTile.Health == health)
            {
                return;
            }

            netTile.SetHealth(health);

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

            if (_entityManager.GetPrefab(structureId) is not Structure)
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

            if (!_tiles.TryGetValue(cell, out Tile tile))
            {
                return;
            }

            Structure structure = (Structure)_entityManager.Spawn(structureId, new SpawnParams() { Parent = tile.transform, Position = new Vector3(tile.transform.position.x, tile.GetSurfaceY(), tile.transform.position.z) });
            structure.SetCell(cell);
            
            netTile.SetStructure(structure);
            _netTiles.SetDirty(cell);
        }

        private void HandleNetTilesChanged(SyncDictionaryChange<Vector2Int, NetTile> change)
        {
            // Tile exists
            if (change.operation != SyncDictionaryOperation.Removed && change.value != null)
            {
                SetTile(change.key, change.value);
            }
            // Tile no longer exists
            else
            {
                RemoveTile(change.key);
            }
        }

        // Adds a new tile, or updates an existing one
        private void SetTile(Vector2Int cell, NetTile netTile)
        {
            // Retrieve from pool
            if (!_tiles.ContainsKey(cell))
            {
                _tiles[cell] = (Tile)_entityManager.Spawn(netTile.TileId, new SpawnParams() { Parent = _tilesContainer });
                _tiles[cell].Initialise(_context);
            }

            Tile tile = _tiles[cell];

            tile.SetHealth(netTile.Health);
            tile.SetCell(cell);
            tile.SetRotations(netTile.Rotations);
            tile.SetStructure(netTile.Structure);

            OnTileChanged?.Invoke(cell, tile);
        }

        private void RemoveTile(Vector2Int cell)
        {
            if (!_tiles.TryGetValue(cell, out Tile tile))
            {
                return;
            }

            // Structures and tiles handle despawning themselves, we just need to remove them from collections
            if (tile.Structure?.isSpawned ?? false)
            {
                tile.Structure.EntityDefeatModule.SetIsDefeated(true);
            }

            tile.SetHealth(0);

            _tiles.Remove(cell);

            OnTileChanged?.Invoke(cell, null);
        }
    }
}