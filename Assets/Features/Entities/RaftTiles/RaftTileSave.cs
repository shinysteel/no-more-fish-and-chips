using Newtonsoft.Json;
using ShinyOwl.Common.Utils;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftTileSave
    {
        [JsonProperty] private SimpleVector2Int _cell = new();
        [JsonProperty] public EntityId TileId { get; private set; }
        [JsonProperty] public int Health { get; private set; }
        [JsonProperty] public int Rotations { get; private set; }

        [JsonIgnore]
        public Vector2Int Cell
        {
            get => _cell.ToVector2Int();
            set => _cell = new SimpleVector2Int(value);
        }

        public RaftTileSave()
        { }

        public RaftTileSave(Vector2Int cell, EntityId tileId, int health, int rotations)
        {
            Cell = cell;
            TileId = tileId;
            Health = health;
            Rotations = rotations;
        }

        public RaftTileSave(RaftTile tile) : this(tile.Cell, tile.EntityDefinitionData.Id, tile.EntityHealthModule.Current, tile.Rotations)
        { }
    }
}