using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Items;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using EntityId = NoMoreFishAndChips.Entities.EntityId;

namespace NoMoreFishAndChips.Environments
{
    public class GameplayEnvironmentSave
    {
        [JsonProperty] public RaftSave Raft { get; private set; } = new();
        [JsonProperty] public List<DroppedItemSave> DroppedItems { get; private set; } = new();

        public void LoadTo(GameplayEnvironment environment)
        {
            EntityManager entityManager = GameManager.Instance.Get<EntityManager>();

            Raft.LoadTo(environment.Context.Raft);

            foreach (DroppedItemSave save in DroppedItems)
            {
                DroppedItem droppedItem = (DroppedItem)entityManager.Spawn(EntityId.DroppedItem, new SpawnParams() { Position = save.Position });
                droppedItem.Set(NetItemInstance.Create(save), save.Type);
            }
        }

        public void SaveFrom(GameplayEnvironment environment)
        {
            EntityManager entityManager = GameManager.Instance.Get<EntityManager>();

            Raft.SaveFrom(environment.Context.Raft);

            DroppedItems.Clear();

            foreach (IEntity entity in entityManager.Entities)
            {
                if (entity is not DroppedItem droppedItem)
                {
                    continue;
                }

                DroppedItems.Add(new DroppedItemSave(droppedItem));
            }
        }

        public void ApplyDefaults()
        {
            Raft.ApplyDefaults();
        }
    }
}