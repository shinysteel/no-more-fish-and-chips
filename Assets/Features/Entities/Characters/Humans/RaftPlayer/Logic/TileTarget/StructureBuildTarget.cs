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
        private EnvironmentManager _environmentManager;

        private EntityId _structureId;
        private Structure _structure;

        private Dictionary<Vector2Int, RaftTile> _overlappingTiles = new();

        private EntityModel _previewModel;
        private List<PreviewProp> _previewProps = new();

        private class PreviewProp
        {
            public Vector2Int Cell { get; private set; }
            public Prop Prop { get; private set; }

            public PreviewProp(Vector2Int cell, Prop prop)
            {
                Cell = cell;
                Prop = prop;
            }
        }

        public StructureBuildTarget(GameplayContext context, BuildTargetSettings settings, EntityId structureId) : base(context, settings)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _environmentManager = GameManager.Instance.Get<EnvironmentManager>();

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

            // For every cell opposite the player's forward and beyond the pivot, offset the pivot 
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

            _overlappingTiles.Clear();

            // Tracking overlappingTiles is neccessary to know where previews should sit on the y-axis
            _structure.StructureDefinitionData.Shape.ForEachCell((Vector2Int cell, bool value) =>
            {
                Vector2Int structureCell = _cell + cell;
                Vector2Int tileCell = _context.Raft.Queries.StructureCellToTileCell(structureCell);

                if (!_overlappingTiles.ContainsKey(tileCell))
                {
                    _context.Raft.Tiles.TryGetValue(tileCell, out RaftTile tile);
                    _overlappingTiles.Add(tileCell, tile);
                }
            });

            foreach (PreviewProp preview in _previewProps)
            {
                _environmentManager.ReturnProp(preview.Prop);
            }

            _previewProps.Clear();

            _structure.StructureDefinitionData.Shape.ForEachTrue((Vector2Int cell) =>
            {
                Vector2Int structureCell = _cell + cell;
                Prop prop = _environmentManager.GetProp(PropId.BoxSelect, new SpawnParams() { Scale = new Vector3(0.51f, 0.26f, 0.51f) });
                _previewProps.Add(new PreviewProp(structureCell, prop));
            });
            
            RefreshPreview();
        }

        private void HandleTileChanged(Vector2Int cell, RaftTile previous, RaftTile current)
        {
            // If a tile we're overlapping has changed, the preview is dirty
            if (!_overlappingTiles.ContainsKey(cell))
            {
                return;
            }

            _overlappingTiles[cell] = current;

            RefreshPreview();
        }

        private void HandleStructureChanged(Vector2Int cell, Structure previous, Structure current)
        {
            // Using the difference, we can know if a structure exists inside the shape
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

            // A refresh involves recalculating the positions and colors of both the previewModel and previewProps

            Vector2 centerCell = new Vector2((_structure.StructureDefinitionData.Shape.GridBounds.xMin + _structure.StructureDefinitionData.Shape.GridBounds.xMax) / 2f, (_structure.StructureDefinitionData.Shape.GridBounds.yMin + _structure.StructureDefinitionData.Shape.GridBounds.yMax) / 2f);
            
            Vector3 modelPosition = _context.Raft.Queries.StructureCellToWorldPosition(_cell + centerCell);
            modelPosition.y = _previewModel.transform.position.y;
            _previewModel.transform.position = modelPosition;

            foreach (PreviewProp preview in _previewProps)
            {
                Vector3 propPosition = _context.Raft.Queries.StructureCellToWorldPosition(preview.Cell);
                propPosition.y = preview.Prop.transform.position.y;
                preview.Prop.transform.position = propPosition;
            }

            Color color = CanBuild() ? _settings.ValidColor : _settings.InvalidColor;

            _previewModel.SetColor(color);

            foreach (PreviewProp preview in _previewProps)
            {
                preview.Prop.SetColor(color);
            }
        }

        public override void Tick()
        {
            PreviewTick();
        }

        private void PreviewTick()
        {
            // Y is constantly updated to have props sit on top of the tiles they overlap

            float? y = null;

            foreach (RaftTile tile in _overlappingTiles.Values)
            {
                if (tile == null)
                {
                    continue;
                }
                
                y = Mathf.Max(y ?? int.MinValue, tile.transform.position.y);
            }

            Vector3 modelPosition = _previewModel.transform.position;
            modelPosition.y = y ?? 0.125f;
            _previewModel.transform.position = modelPosition;

            foreach (PreviewProp preview in _previewProps)
            {
                Vector3 propPosition = preview.Prop.transform.position;
                propPosition.y = modelPosition.y - 0.125f;
                preview.Prop.transform.position = propPosition;
            }
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