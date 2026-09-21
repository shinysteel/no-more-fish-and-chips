using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Structures;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Pool;
using EntityId = NoMoreFishAndChips.Entities.EntityId;

namespace NoMoreFishAndChips.Environments
{
    public class Raft : GameplayBehaviour, IEntityManagerListener
    {
        private SyncDictionaryWrapper<Vector2Int, RaftTile> _netTiles = new SyncDictionaryWrapper<Vector2Int, RaftTile>(ownerAuth: true);
        private SyncDictionaryWrapper<Vector2Int, Structure> _netStructures = new SyncDictionaryWrapper<Vector2Int, Structure>(ownerAuth: true);

        private Dictionary<Vector2Int, RaftTile> _tiles = new();
        private Dictionary<Vector2Int, Structure> _structures = new();

        public IReadOnlyDictionary<Vector2Int, RaftTile> Tiles => _tiles;
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

            foreach (KeyValuePair<Vector2Int, RaftTile> kvp in _netTiles)
            {
                SyncDictionaryChange<Vector2Int, RaftTile> change = new SyncDictionaryChange<Vector2Int, RaftTile>(SyncDictionaryOperation.Added, kvp.Key, kvp.Value);
                HandleNetTilesChanged(change);
            }

            _netTiles.onChanged += HandleNetTilesChanged;

            foreach (KeyValuePair<Vector2Int, Structure> kvp in _netStructures)
            {
                SyncDictionaryChange<Vector2Int, Structure> change = new SyncDictionaryChange<Vector2Int, Structure>(SyncDictionaryOperation.Added, kvp.Key, kvp.Value);
                HandleNetStructuresChanged(change);
            }

            _netStructures.onChanged += HandleNetStructuresChanged;

            _entityManager.AddListener(this);
        }

        protected override void OnDespawned()
        {
            _instantiateManager.RaiseComponentDestroyed(this);

            _queries?.Dispose();

            _netTiles.onChanged -= HandleNetTilesChanged;
             
            _entityManager.RemoveListener(this);
        }

        private void RaiseTileChanged(Vector2Int cell, RaftTile previous, RaftTile current)
        {
            OnTileChanged?.Invoke(cell, previous, current);
        }

        private void RaiseStructureChanged(Vector2Int cell, Structure previous, Structure current)
        {
            OnStructureChanged?.Invoke(cell, previous, current);
        }

        private void HandleNetTilesChanged(SyncDictionaryChange<Vector2Int, RaftTile> change)
        {
            Utils.Network.CacheSyncDictionaryChange(_tiles, change, RaiseTileChanged);
        }

        private void HandleNetStructuresChanged(SyncDictionaryChange<Vector2Int, Structure> change)
        {
            Utils.Network.CacheSyncDictionaryChange(_structures, change, RaiseStructureChanged);
        }

        private RaftTile CreateTile(Vector2Int cell, EntityId tileId, int health, int rotations)
        {
            RaftTile tile = (RaftTile)_entityManager.Spawn(tileId, new SpawnParams() { Parent = transform });

            tile.EntityHealthLogic.SetHealth(health);
            tile.SetNetCell(cell);
            tile.SetNetRotations(rotations);

            return tile;
        }

        [ServerRpc(requireOwnership: false)]
        public void AddTileScaffoldRpc(Vector2Int cell, EntityId buildId, int buildRotations)
        {
            if (_netTiles.ContainsKey(cell))
            {
                return;
            }

            Entity prefab = _entityManager.GetPrefab(EntityId.ScaffoldRaftTile);
            ScaffoldRaftTile tile = (ScaffoldRaftTile)CreateTile(cell, EntityId.ScaffoldRaftTile, prefab.EntityDefinitionData.Health, 0);

            tile.SetNetBuildId(buildId);
            tile.SetNetBuildRotations(buildRotations);

            _netTiles.Add(cell, tile);
        }

        [ServerRpc(requireOwnership: false)]
        public void SetTileRpc(Vector2Int cell, EntityId tileId, int health, int rotations)
        {
            if (_netTiles.TryGetValue(cell, out RaftTile previous))
            {
                _entityManager.Despawn(previous);
            }

            RaftTile current = CreateTile(cell, tileId, health, rotations);

            _netTiles[cell] = current;
        }

        private Structure CreateStructure(Vector2Int cell, EntityId structureId, int health, int rotations)
        {
            Structure structure = (Structure)_entityManager.Spawn(structureId, new SpawnParams() { Parent = transform });

            structure.EntityHealthLogic.SetHealth(health);
            structure.SetNetCell(cell);
            structure.SetNetRotations(rotations);

            return structure;
        }

        [ServerRpc(requireOwnership: false)]
        public void AddStructureScaffoldRpc(Vector2Int addCell, EntityId buildId, int buildRotations)
        {
            Structure prefab = (Structure)_entityManager.GetPrefab(buildId);
            BoolGrid shape = prefab.StructureDefinitionData.Shape.GetTransformed(Vector2Int.zero, buildRotations);

            foreach (KeyValuePair<Vector2Int, bool> kvp in shape)
            {
                if (!kvp.Value)
                {
                    continue;
                }

                if (_netStructures.ContainsKey(addCell + kvp.Key))
                {
                    return;
                }
            }

            StructureScaffold structure = (StructureScaffold)CreateStructure(addCell, EntityId.StructureScaffold, prefab.EntityDefinitionData.Health, 0);

            structure.SetNetBuildId(buildId);
            structure.SetNetBuildRotations(buildRotations);

            shape.ForEachTrue((Vector2Int cell) =>
            {
                _netStructures.Add(addCell + cell, structure);
            });
        }

        [ServerRpc(requireOwnership: false)]
        public void SetStructureRpc(Vector2Int setCell, EntityId structureId, int health, int rotations)
        {
            if (_netStructures.ContainsKey(setCell))
            {
                return;
            }

            Structure setStructure = CreateStructure(setCell, structureId, health, rotations);
            BoolGrid setShape = setStructure.StructureDefinitionData.Shape.GetTransformed(Vector2Int.zero, rotations);

            // Determine and despawn overlapping structures
            List<Structure> overlappingStructures = ListPool<Structure>.Get();

            setShape.ForEachTrue((Vector2Int cell) =>
            {
                if (_structures.TryGetValue(setCell + cell, out Structure structure) && !overlappingStructures.Contains(structure))
                {
                    overlappingStructures.Add(structure);
                }
            });

            foreach (Structure structure in overlappingStructures)
            {
                _entityManager.Despawn(structure);

                BoolGrid shape = structure.StructureDefinitionData.Shape.GetTransformed(Vector2Int.zero, structure.Rotations);

                shape.ForEachTrue((Vector2Int cell) =>
                {
                    _netStructures[structure.Cell + cell] = null;
                });
            }

            ListPool<Structure>.Release(overlappingStructures);

            setShape.ForEachTrue((Vector2Int cell) =>
            {
                _netStructures[setCell + cell] = setStructure;
            });
        }

        void IEntityManagerListener.OnEntityDespawned(Entity entity)
        {
            if (!isOwner)
            {
                return;
            }

            if (entity is RaftTile tile)
            {
                _netTiles.Remove(tile.Cell);

                if (isOwner && _netTiles.Count > 0)
                {
                    DefeatDisconnectedTiles();
                }
            }
            else if (entity is Structure structure)
            {
                BoolGrid shape = structure.StructureDefinitionData.Shape.GetTransformed(Vector2Int.zero, structure.Rotations);

                shape.ForEachTrue((Vector2Int cell) =>
                {
                    _netStructures.Remove(structure.Cell + cell);
                });
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

            foreach (RaftTile tile in _netTiles.Values)
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

                    if (!_netTiles.TryGetValue(cell, out RaftTile tile))
                    {
                        return;
                    }

                    visitedCells.Add(cell);

                    if (tile.TileDefinitionData.IsScaffold)
                    {
                        return;
                    }

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