using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Saving;
using Newtonsoft.Json;
using System;
using UnityEngine;
using ShinyOwl.Common.Utils;
using PurrNet;

namespace NoMoreFishAndChips.Entities
{
    public abstract class Structure : Entity
    {
        private SyncVar<Vector2Int> _netCell = new SyncVar<Vector2Int>(ownerAuth: true);
        private SyncVar<int> _netRotations = new SyncVar<int>(ownerAuth: true);

        public Vector2Int Cell => _netCell.value;
        public int Rotations => _netRotations.value;

        public StructureDefinitionData StructureDefinitionData => (StructureDefinitionData)_entityDefinitionData;

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (isOwner)
            {
                transform.position = _context.Raft.Queries.StructureCellToWorldPosition(_netCell.value);
                transform.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
            }
        }

        public void SetNetCell(Vector2Int cell)
        {
            _netCell.value = cell;
        }

        public void SetNetRotations(int rotations)
        {
            _netRotations.value = rotations;
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