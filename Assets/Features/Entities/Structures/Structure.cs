using FishFlingers.Items;
using FishFlingers.Saving;
using Newtonsoft.Json;
using System;
using UnityEngine;
using ShinyOwl.Common.Utils;

namespace FishFlingers.Entities
{
    public class StructureSave
    {
        [JsonProperty] private SimpleVector2Int _cell = new();
        [JsonProperty] public EntityId StructureId { get; private set; }

        [JsonIgnore] public Vector2Int Cell
        {
            get => _cell.ToVector2Int();
            set => _cell = new SimpleVector2Int(value);
        }

        public StructureSave(Vector2Int cell, EntityId structureId)
        {
            Cell = cell;
            StructureId = structureId;
        }
    }

    public abstract class Structure : NetEntity
    {
        public StructureData StructureData => (StructureData)_entityData;
    }

    public abstract class Structure<T> : Structure where T : StructureData
    {
        public T Data => (T)_entityData;
    }
}