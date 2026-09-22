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
using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;

namespace NoMoreFishAndChips.Environments
{
    public class Raft : GameplayBehaviour, IEntityManagerListener
    {
        private SyncDictionaryWrapper<Vector2Int, RaftTile> _netTiles = new SyncDictionaryWrapper<Vector2Int, RaftTile>(ownerAuth: true);
        private SyncList<Structure> _netStructures = new SyncList<Structure>(ownerAuth: true);

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

            for (int i = 0; i < _netStructures.Count; i++)
            {
                SyncListChange<Structure> change = SyncListChange<Structure>.Added(_netStructures[i], i);
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

            _netStructures.onChanged -= HandleNetStructuresChanged;
             
            _entityManager.RemoveListener(this);
        }

        private void RaiseTileChanged(Vector2Int cell, RaftTile previous, RaftTile current)
        {
            OnTileChanged?.Invoke(cell, previous, current);
        }

        private void HandleNetTilesChanged(SyncDictionaryChange<Vector2Int, RaftTile> change)
        {
            Utils.Network.CacheSyncDictionaryChange(_tiles, change, RaiseTileChanged);
        }

        private void HandleNetStructuresChanged(SyncListChange<Structure> change)
        {
            void add(Structure structure)
            {
                structure.Shape.ForEachTrue((Vector2Int shapeCell) =>
                {
                    Vector2Int structureCell = structure.Cell + shapeCell;
                    _structures.Add(structureCell, structure);
                    OnStructureChanged?.Invoke(structureCell, null, structure);
                });
            }

            void remove(Structure structure)
            {
                structure.Shape.ForEachTrue((Vector2Int shapeCell) =>
                {
                    Vector2Int structureCell = structure.Cell + shapeCell;
                    _structures.Remove(structureCell);
                    OnStructureChanged?.Invoke(structureCell, structure, null);
                });
            }

            switch (change.operation)
            {
                case SyncListOperation.Added:
                case SyncListOperation.Insert:
                    add(change.value);
                    break;

                case SyncListOperation.Removed:
                    remove(change.value);
                    break;

                case SyncListOperation.Set:
                    if (change.oldValue != null)
                    {
                        remove(change.oldValue);
                    }

                    if (change.value != null)
                    {
                        add(change.value);
                    }
                    break;

                case SyncListOperation.Cleared:
                    Dictionary<Vector2Int, Structure> dictionary = DictionaryPool<Vector2Int, Structure>.Get();

                    foreach (KeyValuePair<Vector2Int, Structure> kvp in _structures)
                    {
                        dictionary.Add(kvp.Key, kvp.Value);
                    }

                    _structures.Clear();

                    foreach (KeyValuePair<Vector2Int, Structure> kvp in dictionary)
                    {
                        OnStructureChanged?.Invoke(kvp.Key, kvp.Value, null);
                    }

                    DictionaryPool<Vector2Int, Structure>.Release(dictionary);
                    break;
            }
        }

        private RaftTile CreateTile(Vector2Int cell, EntityId tileId, int health, int rotations)
        {
            RaftTile tile = (RaftTile)_entityManager.Spawn(tileId, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition, Parent = transform });

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
            Structure structure = (Structure)_entityManager.Spawn(structureId, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition, Parent = transform });

            structure.EntityHealthLogic.SetHealth(health);
            structure.SetNetCell(cell);
            structure.SetNetRotations(rotations);

            return structure;
        }

        [ServerRpc(requireOwnership: false)]
        public void AddStructureScaffoldRpc(Vector2Int cell, EntityId buildId, int buildRotations)
        {
            Structure prefab = (Structure)_entityManager.GetPrefab(buildId);
            BoolGrid shape = prefab.StructureDefinitionData.Shape.GetTransformed(Vector2Int.zero, buildRotations);

            foreach (KeyValuePair<Vector2Int, bool> kvp in shape)
            {
                if (!kvp.Value)
                {
                    continue;
                }

                Vector2Int structureCell = cell + kvp.Key;

                if (_structures.ContainsKey(structureCell))
                {
                    return;
                }

                Vector2Int tileCell = _queries.StructureCellToTileCell(structureCell);

                if (!_tiles.ContainsKey(tileCell))
                {
                    return;
                }
            }

            StructureScaffold structure = (StructureScaffold)CreateStructure(cell, EntityId.StructureScaffold, prefab.EntityDefinitionData.Health, 0);

            structure.SetNetBuildId(buildId);
            structure.SetNetBuildRotations(buildRotations);

            _netStructures.Add(structure);
        }

        [ServerRpc(requireOwnership: false)]
        public void SetStructureRpc(Vector2Int setCell, EntityId structureId, int health, int rotations)
        {
            Structure setStructure = CreateStructure(setCell, structureId, health, rotations);

            // Determine and despawn overlapping structures
            List<Structure> overlappingStructures = ListPool<Structure>.Get();

            setStructure.Shape.ForEachTrue((Vector2Int shapeCell) =>
            {
                if (_structures.TryGetValue(setCell + shapeCell, out Structure structure) && !overlappingStructures.Contains(structure))
                {
                    overlappingStructures.Add(structure);
                }
            });

            foreach (Structure structure in overlappingStructures)
            {
                _entityManager.Despawn(structure);
            }

            ListPool<Structure>.Release(overlappingStructures);

            _netStructures.Add(setStructure);
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
                _netStructures.Remove(structure);
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