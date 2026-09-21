using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Saving;
using Newtonsoft.Json;
using System;
using UnityEngine;
using ShinyOwl.Common.Utils;
using PurrNet;
using ShinyOwl.Common.Structures;
using ShinyOwl.Common;

namespace NoMoreFishAndChips.Entities
{
    public abstract class Structure : Entity
    {
        protected SyncVar<Vector2Int> _netCell = new SyncVar<Vector2Int>(ownerAuth: true);
        private SyncVar<int> _netRotations = new SyncVar<int>(ownerAuth: true);

        public Vector2Int Cell => _netCell.value;
        public int Rotations => _netRotations.value;

        protected BoolGrid _shape;
        public BoolGrid Shape => _shape;

        public StructureDefinitionData StructureDefinitionData => (StructureDefinitionData)_entityDefinitionData;

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (isOwner && !StructureDefinitionData.IsScaffold)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
            }

            HandleNetCellChanged(_netCell.value);
            HandleNetRotationsChanged(_netRotations.value);

            _netCell.onChanged += HandleNetCellChanged;
            _netRotations.onChanged += HandleNetRotationsChanged;
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _netCell.onChanged -= HandleNetCellChanged;
            _netRotations.onChanged -= HandleNetRotationsChanged;
        }

        protected virtual void RefreshShape()
        {
            _shape = StructureDefinitionData.Shape.GetTransformed(Vector2Int.zero, _netRotations.value);
        }

        private void HandleNetCellChanged(Vector2Int cell)
        {
            if (isOwner)
            {
                Vector3 position = _context.Raft.Queries.StructureCellToWorldPosition(cell);
                position.y = 0.125f;
                transform.position = position;
            }
        }

        private void HandleNetRotationsChanged(int rotations)
        {
            RefreshShape();
        }

        public void SetNetCell(Vector2Int cell)
        {
            _netCell.value = cell;
        }

        public void SetNetRotations(int rotations)
        {
            _netRotations.value = rotations;

            // Raft needs _shape to be assigned as soon as the Structure is created
            if (isOwner)
            {
                HandleNetRotationsChanged(_netRotations.value);
            }
        }

        public virtual string GetJsonData()
        {
            return null;
        }

        public virtual void LoadJsonData(string json)
        { }
    }

    public abstract class Structure<T> : Structure where T : StructureDefinitionData
    {
        public T DefinitionData => (T)_entityDefinitionData;
    }
}