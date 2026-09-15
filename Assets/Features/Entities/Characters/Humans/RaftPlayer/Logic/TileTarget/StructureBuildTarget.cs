using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using NUnit.Framework;
using UnityEngine;
using NoMoreFishAndChips.Environments;
using System.Collections.Generic;
using ShinyOwl.Common;

namespace NoMoreFishAndChips.Entities
{
    public class StructureBuildTarget : BuildTarget
    {
        private EntityManager _entityManager;

        private EntityId _structureId;

        private Structure _structure;

        private Dictionary<Vector2Int, RaftTile> _tiles = new();

        private EntityModel _previewModel;

        public StructureBuildTarget(GameplayContext context, BuildTargetSettings settings, EntityId structureId) : base(context, settings)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();

            _structureId = structureId;

            _structure = (Structure)_entityManager.GetPrefab(_structureId);

            _previewModel = _entityManager.GetModel(structureId, new SpawnParams() { Rotation = Quaternion.LookRotation(Vector3.back, Vector3.up) });

            RefreshPreview();

            _context.Raft.OnTileChanged += HandleTileChanged;
            _context.Raft.OnStructureChanged += HandleStructureChanged;
        }

        public override void Dispose()
        {
            _entityManager.ReturnModel(_previewModel);

            if (_context.Raft != null)
            {
                _context.Raft.OnTileChanged -= HandleTileChanged;
                _context.Raft.OnStructureChanged -= HandleStructureChanged;
            }
        }

        public override void SetPosition(Vector3 position)
        {
            if (_position == position)
            {
                return;
            }
            
            _position = position;

            Vector2Int targetCell = _context.Raft.Queries.WorldPositionToStructureCell(position);

            Vector2Int playerCell = _context.Raft.Queries.WorldPositionToStructureCell(_context.LocalPlayer.transform.position);
            Vector2Int direction = targetCell - playerCell;

            if (direction.x != 0)
            {
                targetCell.x -= direction.x > 0 ? _structure.StructureDefinitionData.Shape.GridBounds.xMin : _structure.StructureDefinitionData.Shape.GridBounds.xMax;
            }

            if (direction.y != 0)
            {
                targetCell.y -= direction.y > 0 ? _structure.StructureDefinitionData.Shape.GridBounds.yMin : _structure.StructureDefinitionData.Shape.GridBounds.yMax;
            }

            if (_cell == targetCell)
            {
                return;
            }

            _cell = targetCell;

            _tiles.Clear();

            _structure.StructureDefinitionData.Shape.ForEachCell((Vector2Int cell, bool value) =>
            {
                Vector2Int structureCell = _cell + cell;
                Vector2Int tileCell = _context.Raft.Queries.StructureCellToTileCell(structureCell);

                _context.Raft.Tiles.TryGetValue(tileCell, out RaftTile tile);
                _tiles.Add(tileCell, tile);
            });
            
            RefreshPreview();
        }

        private void HandleTileChanged(Vector2Int cell, RaftTile previous, RaftTile current)
        {
            if (!_tiles.ContainsKey(cell))
            {
                return;
            }

            _tiles[cell] = current;

            RefreshPreview();
        }

        private void HandleStructureChanged(Vector2Int cell, Structure previous, Structure current)
        {
            if (_structure.StructureDefinitionData.Shape[cell - _cell] == true)
            {
                RefreshPreview();
            }
        }

        private void RefreshPreview()
        {   
            if (_previewModel == null)
            {
                return;
            }

            Vector2 centerCell = new Vector2((_structure.StructureDefinitionData.Shape.GridBounds.xMin + _structure.StructureDefinitionData.Shape.GridBounds.xMax) / 2f, (_structure.StructureDefinitionData.Shape.GridBounds.yMin + _structure.StructureDefinitionData.Shape.GridBounds.yMax) / 2f);
            Vector2 structureCell = _cell + centerCell;
            Vector3 position = _context.Raft.Queries.StructureCellToWorldPosition(structureCell);
            
            position.y = 0.125f;

            _previewModel.transform.position = position;

            _previewModel.SetMaterialColor(CanBuild() ? _settings.ValidColor : _settings.InvalidColor);
        }

        public override void Tick()
        {
            PreviewTick();
        }

        private void PreviewTick()
        {
            float? y = null;

            foreach (RaftTile tile in _tiles.Values)
            {
                if (tile == null)
                {
                    continue;
                }
                
                y = Mathf.Max(y ?? int.MinValue, tile.transform.position.y);
            }

            Vector3 position = _previewModel.transform.position;
            position.y = y ?? 0.125f;

            _previewModel.transform.position = position;
        }

        protected override bool CanBuild()
        {
            bool build = true;

            _structure.StructureDefinitionData.Shape.ForEachTrue((Vector2Int cell) =>
            {
                if (!build)
                {
                    return;
                }

                Vector2Int structureCell = _cell + cell;
                Vector2Int tileCell = _context.Raft.Queries.StructureCellToTileCell(structureCell);

                if (!_context.Raft.Tiles.ContainsKey(tileCell) || _context.Raft.Structures.ContainsKey(structureCell))
                {
                    build = false;
                    return;
                }
            });
            
            return build;
        }
    }
}