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
    public class Raft : GameplayBehaviour, IEntityManagerListener
    {
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
                foreach (Entity entity in _entityManager.Entities)
                {
                    ((IEntityManagerListener)this).OnEntitySpawned(entity);
                }
            }

            _entityManager.AddListener(this);
        }

        protected override void OnDespawned()
        {
            _instantiateManager.RaiseComponentDestroyed(this);

            _entityManager.RemoveListener(this);

            _queries?.Dispose();
        }

        [ServerRpc(requireOwnership: false)]
        public void AddTileRpc(Vector2Int cell, EntityId tileId, int health, int rotations)
        {
            if (_tiles.ContainsKey(cell))
            {
                return;
            }

            RaftTile tile = (RaftTile)_entityManager.Spawn(tileId, new SpawnParams() { Parent = transform });

            tile.InitialiseContext(_context);
            tile.EntityHealthLogic.SetHealth(health);
            tile.SetNetCell(cell);
            tile.SetNetRotations(rotations);

            _tiles.Add(cell, tile);

            OnTileChanged?.Invoke(cell, null, tile);
        }

        void IEntityManagerListener.OnEntitySpawned(Entity entity)
        {
            if (isOwner)
            {
                return;
            }

            if (entity is RaftTile tile)
            {
                _tiles.Add(tile.Cell, tile);

                OnTileChanged?.Invoke(tile.Cell, null, tile);
            }
        }

        void IEntityManagerListener.OnEntityDespawned(Entity entity)
        {
            if (entity is RaftTile tile)
            {
                _tiles.Remove(tile.Cell);

                if (isOwner && _tiles.Count > 0)
                {
                    DefeatDisconnectedTiles();
                }

                OnTileChanged?.Invoke(tile.Cell, tile, null);
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
                        Log.Info($"defeating disconnected tile at cell {tile.Cell}");
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
    }
}