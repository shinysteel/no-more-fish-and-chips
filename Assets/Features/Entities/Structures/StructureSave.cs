using NoMoreFishAndChips.Entities;
using Newtonsoft.Json;
using ShinyOwl.Common.Utils;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class StructureSave
    {
        [JsonProperty] private SimpleVector2Int _cell = new();
        [JsonProperty] public EntityId StructureId { get; private set; }
        [JsonProperty] public int Health { get; private set; }
        [JsonProperty] public int Rotations { get; private set; }
        [JsonProperty] public string JsonData { get; private set; }

        [JsonIgnore]
        public Vector2Int Cell
        {
            get => _cell.ToVector2Int();
            set => _cell = new SimpleVector2Int(value);
        }

        public StructureSave()
        { }

        public StructureSave(Vector2Int cell, EntityId structureId, int health, int rotations, string jsonData)
        {
            Cell = cell;
            StructureId = structureId;
            Health = health;
            Rotations = rotations;
            JsonData = jsonData;
        }

        public StructureSave(Structure structure) : this(structure.Cell, structure.StructureDefinitionData.Id, structure.EntityHealthLogic.CurrentHealth, structure.Rotations, structure.GetJsonData())
        { }
    }
}