using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using PurrNet;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using EntityId = NoMoreFishAndChips.Entities.EntityId;

namespace NoMoreFishAndChips.Environments
{
    public class Raft : GameplayBehaviour, IEntityManagerListener
    {
        private Dictionary<Vector2Int, RaftTile> _tiles = new();
        public IReadOnlyDictionary<Vector2Int, RaftTile> Tiles => _tiles;
        private Dictionary<Vector2Int, Structure> _structures = new();
        public IReadOnlyDictionary<Vector2Int, Structure> Structures => _structures;

        private RaftQueries _queries;
        public RaftQueries Queries => _queries;

        public event Action<Vector2Int, RaftTile, RaftTile> OnTileChanged;
        public event Action<Vector2Int, Structure, Structure> OnStructureChanged;

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

        private T CreateTile<T>(Vector2Int cell, EntityId tileId, int health, int rotations, Action<T> onCreate) where T : RaftTile
        {
            T tile = (T)_entityManager.Spawn(tileId, new SpawnParams() { Parent = transform });

            tile.InitialiseContext(_context);
            tile.EntityHealthLogic.SetHealth(health);
            tile.SetNetCell(cell);
            tile.SetNetRotations(rotations);

            onCreate?.Invoke(tile);

            return tile;
        }

        [ServerRpc(requireOwnership: false)]
        public void AddTileScaffoldRpc(Vector2Int cell, EntityId buildId, int buildRotations)
        {
            if (_tiles.ContainsKey(cell))
            {
                return;
            }

            ScaffoldRaftTile prefab = (ScaffoldRaftTile)_entityManager.GetPrefab(EntityId.ScaffoldRaftTile);

            ScaffoldRaftTile tile = CreateTile(cell, EntityId.ScaffoldRaftTile, prefab.EntityDefinitionData.Health, 0, (ScaffoldRaftTile tile) =>
            {
                tile.SetNetBuildId(buildId);
                tile.SetNetBuildRotations(buildRotations);
            });

            _tiles.Add(cell, tile);
            OnTileChanged?.Invoke(cell, null, tile);
        }

        public void SetTile(Vector2Int cell, EntityId tileId, int health, int rotations)
        {
            RaftTile previous = _tiles.GetValueOrDefault(cell);
            RaftTile current = CreateTile<RaftTile>(cell, tileId, health, rotations, null);

            _tiles[cell] = current;
            OnTileChanged?.Invoke(cell, previous, current);

            if (previous != null)
            {
                _entityManager.Despawn(previous);
            }
        }

        [ServerRpc(requireOwnership: false)]
        public void AddStructureScaffoldRpc()
        {

        }

        [ServerRpc(requireOwnership: false)]
        public void AddStructureRpc(Vector2Int cell, EntityId structureId)
        {
            if (_structures.ContainsKey(cell))
            {
                return;
            }

            Structure structure = (Structure)_entityManager.Spawn(structureId, new SpawnParams() { Parent = transform });

            structure.SetCell(cell);

            _structures.Add(cell, structure);

            OnStructureChanged?.Invoke(cell, null, structure);
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
            else if (entity is Structure structure)
            {
                _structures.Add(structure.Cell, structure);

                OnStructureChanged?.Invoke(structure.Cell, null, structure);
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
            else if (entity is Structure structure)
            {
                _structures.Remove(structure.Cell);

                OnStructureChanged?.Invoke(structure.Cell, structure, null);
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
    }
}