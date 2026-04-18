using FishFlingers.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using EntityId = FishFlingers.Entities.EntityId;

namespace FishFlingers.Environments
{
    public class RaftSave
    {
        [JsonProperty] public List<TileSave> Tiles { get; private set; } = new();
        [JsonProperty] public List<StructureSave> Structures { get; private set; } = new();

        public void LoadTo(Raft raft)
        {
            foreach (TileSave save in Tiles)
            {
                raft.AddNetTileRpc(save.Cell, save.TileId, save.Health, save.Rotations);
            }

            foreach (StructureSave save in Structures)
            {
                raft.AddStructureRpc(save.Cell, save.StructureId);

                // Since we are the server, we can assume it exists straight away
                raft.Tiles[save.Cell].Structure.LoadJsonData(save.JsonData);
            }
        }

        public void SaveFrom(Raft raft)
        {
            Tiles.Clear();
            Structures.Clear();

            foreach (Tile tile in raft.Tiles.Values)
            {
                Tiles.Add(new TileSave(tile));

                if (tile.Structure != null)
                {
                    Structures.Add(new StructureSave(tile.Structure));
                }
            }
        }

        public void ApplyDefaults()
        {
            // Start with a 3x3 grid
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int health = NetTile.MaxHealth;

                    // 33% chance to have one less health
                    if (Random.value < 1f / 3f)
                    {
                        health--;
                    }

                    int rotations = Random.Range(0, 4);

                    Tiles.Add(new TileSave(new Vector2Int(x, y), EntityId.WoodenTile, health, rotations));
                }
            }

            // Start with a wave sign
            Structures.Add(new StructureSave(new Vector2Int(0, 1), EntityId.WaveSign, string.Empty));
        }
    }
}