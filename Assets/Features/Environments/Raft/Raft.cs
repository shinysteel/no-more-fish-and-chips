using LiteNetLib;
using Newtonsoft.Json;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.Saving;
using NoMoreFishAndChips.Scenes;
using NoMoreFishAndChips.States;
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
    public class Raft : GameplayBehaviour
    {
        private SyncDictionaryWrapper<Vector2Int, RaftTile> _netTiles = new SyncDictionaryWrapper<Vector2Int, RaftTile>(ownerAuth: true);
        private Dictionary<Vector2Int, RaftTile> _tiles = new();
        public IReadOnlyDictionary<Vector2Int, RaftTile> Tiles => _tiles;

        public event Action<Vector2Int, RaftTile> OnTileChanged;

        private RaftQueries _queries;
        public RaftQueries Queries => _queries;

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            // Following this, SaveManager will load a RaftSave
            _instantiateManager.RaiseComponentInstantiated(this);

            _queries = new RaftQueries(this);

            if (!isOwner)
            {
                // Clients need to manually handle changes that have happened before we joined
                foreach (KeyValuePair<Vector2Int, RaftTile> kvp in _netTiles)
                {
                    SyncDictionaryChange<Vector2Int, RaftTile> change = new SyncDictionaryChange<Vector2Int, RaftTile>(SyncDictionaryOperation.Added, kvp.Key, kvp.Value);
                    HandleNetTilesChanged(change);
                }
            }

            _netTiles.onChanged += HandleNetTilesChanged;
        }

        protected override void OnDespawned()
        {
            _instantiateManager.RaiseComponentDestroyed(this);
        }

        private void HandleNetTilesChanged(SyncDictionaryChange<Vector2Int, RaftTile> change)
        {
            if (change.operation == SyncDictionaryOperation.Added)
            {
                _tiles.Add(change.key, change.value);
            }
            else if (change.operation == SyncDictionaryOperation.Removed)
            {
                _tiles.Remove(change.key);
            }

            OnTileChanged?.Invoke(change.key, change.value);
        }

        [ServerRpc(requireOwnership: false)]
        public void AddNetTileRpc(Vector2Int cell, EntityId tileId, int health, int rotations)
        {
            if (_netTiles.ContainsKey(cell))
            {
                return;
            }

            RaftTile tile = (RaftTile)_entityManager.Spawn(tileId, new SpawnParams() { Parent = transform });

            tile.InitialiseContext(_context);
            tile.EntityHealthLogic.SetHealth(health);
            tile.SetNetCell(cell);
            tile.SetNetRotations(rotations);

            _netTiles[cell] = tile;
        }

        public void RemoveNetRaftTile(Vector2Int cell)
        {
            _entityManager.Despawn(_netTiles[cell]);
            _netTiles.Remove(cell);
        }
    }
}