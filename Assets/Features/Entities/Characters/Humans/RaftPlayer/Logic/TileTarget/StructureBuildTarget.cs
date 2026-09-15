using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using NUnit.Framework;
using UnityEngine;
using NoMoreFishAndChips.Environments;
using System.Collections.Generic;
using Unity.VisualScripting;
using ShinyOwl.Common;

namespace NoMoreFishAndChips.Entities
{
    public class StructureBuildTarget : BuildTarget
    {
        private EntityManager _entityManager;
        private EnvironmentManager _environmentManager;

        private EntityId _structureId;

        private Structure _structure;

        private GameObject _previewGameObject;
        private List<Prop> _previewProps = new();

        public StructureBuildTarget(GameplayContext context, BuildTargetSettings settings, Vector3 position, EntityId structureId) : base(context, settings, position)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _environmentManager = GameManager.Instance.Get<EnvironmentManager>();

            _structureId = structureId;

            _structure = (Structure)_entityManager.GetPrefab(_structureId);

            _previewGameObject = new GameObject(nameof(StructureBuildTarget));

            RefreshPreview();

            _context.Raft.OnStructureChanged += HandleStructureChanged;
        }

        public override void Dispose()
        {
            Object.Destroy(_previewGameObject);

            if (_context.Raft != null)
            {
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

            if (_structure != null)
            {
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
            }

            if (_cell == targetCell)
            {
                return;
            }

            _cell = targetCell;

            RefreshPreview();

            // _context.Raft.Structures.TryGetValue(_cell, out Structure structure);

            // HandleStructureChanged(_cell, null, structure);
        }

        private void HandleStructureChanged(Vector2Int cell, Structure previous, Structure current)
        { 

        }

        private void RefreshPreview()
        {   
            if (_previewGameObject == null)
            {
                return;
            }

            _previewGameObject.transform.position = _context.Raft.Queries.StructureCellToWorldPosition(_cell) + Vector3.up * 0.125f;

            foreach (Prop prop in _previewProps)
            {
                _environmentManager.ReturnProp(prop);
            }

            _previewProps.Clear();

            _structure.StructureDefinitionData.Shape.ForEachTrue((Vector2Int cell) =>
            {
                void processSide(Vector3 direction)
                {
                    Vector2Int offset = new Vector2Int((int)direction.x, (int)direction.z);
                    Vector3 position = new Vector3(cell.x, 0f, cell.y) * 0.5f + direction * 0.25f;
                    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

                    if (_structure.StructureDefinitionData.Shape[cell + offset] != true)
                    {
                        Prop tape = _environmentManager.GetProp(PropId.ScaffoldTape, new SpawnParams() { Position = position, Rotation = rotation, Parent = _previewGameObject.transform });
                        _previewProps.Add(tape);
                    }
                }

                processSide(Vector3.forward);
                processSide(Vector3.right);
                processSide(Vector3.back);
                processSide(Vector3.left);
            });

            Color color = CanBuild() ? _settings.ValidColor : _settings.InvalidColor;

            foreach (Prop prop in _previewProps)
            {
                prop.SetColor(color);
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