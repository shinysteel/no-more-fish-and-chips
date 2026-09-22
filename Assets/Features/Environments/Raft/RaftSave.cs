using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Inventories;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using EntityId = NoMoreFishAndChips.Entities.EntityId;

namespace NoMoreFishAndChips.Environments
{
    public class RaftSave
    {
        [JsonProperty] public List<RaftTileSave> Tiles { get; private set; } = new();
        [JsonProperty] public List<StructureSave> Structures { get; private set; } = new();

        public void LoadTo(Raft raft)
        {
            foreach (RaftTileSave save in Tiles)
            {
                raft.SetTileRpc(save.Cell, save.TileId, save.Health, save.Rotations);
            }

            foreach (StructureSave save in Structures)
            {
                raft.SetStructureRpc(save.Cell, save.StructureId, save.Health, save.Rotations);

                // Since we are the server, we can assume it exists straight away
                raft.Structures[save.Cell].LoadJsonData(save.JsonData);
            }
        }

        public void SaveFrom(Raft raft)
        {
            Tiles.Clear();
            Structures.Clear();

            foreach (RaftTile tile in raft.Tiles.Values)
            {
                Tiles.Add(new RaftTileSave(tile));
            }

            foreach (Structure structure in raft.Structures.Values)
            {
                Structures.Add(new StructureSave(structure));
            }
        }

        public void ApplyDefaults()
        {
            EntityManager entityManager = GameManager.Instance.Get<EntityManager>();

            // Start with a 3x3 grid
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    EntityId id = EntityId.WoodenRaftTile;

                    EntityDefinitionData data = entityManager.GetPrefab(id).EntityDefinitionData;

                    int health = data.Health;

                    // 33% chance to have one less health
                    //if (Random.value < 1f / 3f)
                    //{
                    //    health--;
                    //}

                    int rotations = Random.Range(0, 4);

                    Tiles.Add(new RaftTileSave(new Vector2Int(x, y), id, health, rotations));
                }
            }

            // Start with a wave sign
            Entity prefab = entityManager.GetPrefab(EntityId.WaveCounter);
            Structures.Add(new StructureSave(new Vector2Int(0, 3), EntityId.WaveCounter, prefab.EntityDefinitionData.Health, 0, string.Empty));
        }
    }
}