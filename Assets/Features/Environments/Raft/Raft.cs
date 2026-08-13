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
using UnityEngine.Pool;
using EntityId = NoMoreFishAndChips.Entities.EntityId;

namespace NoMoreFishAndChips.Environments
{
    public class Raft : GameplayBehaviour
    {
        private SyncDictionaryWrapper<Vector2Int, RaftTile> _netTiles = new SyncDictionaryWrapper<Vector2Int, RaftTile>(ownerAuth: true);
        private Dictionary<Vector2Int, RaftTile> _tiles = new();
        public IReadOnlyDictionary<Vector2Int, RaftTile> Tiles => _tiles;

        public event Action<Vector2Int, RaftTile, RaftTile> OnTileChanged;

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

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _queries?.Dispose();
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
                OnTileChanged?.Invoke(change.key, null, change.value);
            }
            else if (change.operation == SyncDictionaryOperation.Removed)
            {
                _tiles.Remove(change.key);

                if (isOwner && _tiles.Count > 0)
                {
                    DefeatDisconnectedTiles();
                }

                // Despite the operation, change.value is not null and so it needs to be entered manually
                OnTileChanged?.Invoke(change.key, change.value, null);
            }
        }

        private void DefeatDisconnectedTiles()
        {
            List<List<RaftTile>> tileGroups = ListPool<List<RaftTile>>.Get();

            DetermineTileGroups(tileGroups);

            if (tileGroups.Count > 1)
            {
                List<RaftTile> mainGroup = tileGroups
                    .OrderByDescending(group => group.Count)
                    .ThenBy(group => group.Min(tile => tile.Cell.x))
                    .ThenBy(group => group.Min(tile => tile.Cell.y))
                    .First();

                int mainIndex = tileGroups.IndexOf(mainGroup);

                for (int i = 0; i < tileGroups.Count; i++)
                {
                    if (i == mainIndex)
                    {
                        continue;
                    }

                    foreach (RaftTile tile in tileGroups[i])
                    {
                        tile.EntityDefeatLogic.SetIsDefeated(true);
                    }
                }
            }

            foreach (List<RaftTile> group in tileGroups)
            {
                ListPool<RaftTile>.Release(group);
            }

            ListPool<List<RaftTile>>.Release(tileGroups);
        }

        private void DetermineTileGroups(List<List<RaftTile>> tileGroups)
        {
            List<Vector2Int> visitedCells = ListPool<Vector2Int>.Get();

            foreach (RaftTile tile in _tiles.Values)
            {
                if (visitedCells.Contains(tile.Cell))
                {
                    continue;
                }

                List<RaftTile> group = ListPool<RaftTile>.Get();

                void processCell(Vector2Int cell)
                {
                    if (visitedCells.Contains(cell))
                    {
                        return;
                    }

                    if (!_tiles.TryGetValue(cell, out RaftTile tile))
                    {
                        return;
                    }

                    visitedCells.Add(cell);
                    group.Add(tile);

                    for (int i = -1; i <= 1; i += 2)
                    {
                        processCell(cell + new Vector2Int(i, 0));
                        processCell(cell + new Vector2Int(0, i));
                    }
                }

                processCell(tile.Cell);
                
                tileGroups.Add(group);
            }

            ListPool<Vector2Int>.Release(visitedCells);
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
            RaftTile tile = _netTiles[cell];

            _netTiles.Remove(cell);

            _entityManager.Despawn(tile);
        }

        public void ClearNetRaftTiles()
        {
            foreach (Vector2Int cell in _netTiles.Keys.ToArray())
            {
                RemoveNetRaftTile(cell);
            }
        }
    }
}