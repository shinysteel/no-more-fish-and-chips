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
        [JsonProperty] public SimpleVector2Int Cell { get; private set; }
        [JsonProperty] public EntityId StructureId { get; private set; }

        public StructureSave(Vector2Int cell, EntityId structureId)
        {
            Cell = new SimpleVector2Int(cell);
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