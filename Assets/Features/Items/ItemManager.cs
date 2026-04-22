using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.Networking;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using EntityId = FishFlingers.Entities.EntityId;

namespace FishFlingers.Items
{
    public interface IItemManagerListener
    { }

    public class ItemManager : GameSystem<IItemManagerListener>
    {
        private EntityManager _entityManager;

        private ItemManagerConfig _config;

        private Dictionary<ItemId, ItemData> _idDataMap = new();

        public override void Initialise(GameManagerConfig config)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();

            _config = config.ItemManagerConfig;

            foreach (ItemData data in _config.ItemDataScanner.GetAssets())
            {
                _idDataMap.Add(data.ItemId, data);
            }

            base.Initialise(config);
        }

        public ItemData GetItemData(ItemId id)
        {
            return _idDataMap[id];
        }

        public void SpawnDrop(Vector3 position, DropTable table)
        {
            SpawnDrops(position, new DropTable[] { table });
        }

        public void SpawnDrops(Vector3 position, DropTable[] tables)
        {
            List<WeightedPick<ItemId>> picks = ListPool<WeightedPick<ItemId>>.Get();
            List<NetItemInstance> netItemInstances = ListPool<NetItemInstance>.Get();

            try
            {
                // Pick each table
                foreach (DropTable table in tables)
                {
                    WeightedPicker<ItemId> picker = new();
                    picker.Set(table.Entries);
                    WeightedPick<ItemId> pick = picker.Pick();

                    if (pick.Value != ItemId.None)
                    {
                        picks.Add(pick);
                    }
                }

                if (picks.Count == 0)
                {
                    return;
                }
                
                // Enforce max stack for each pick
                foreach (WeightedPick<ItemId> pick in picks)
                {
                    ItemData data = GetItemData(pick.Value);
                    int pickCount = pick.Count;

                    while (pickCount > 0)
                    {
                        int itemCount = Mathf.Min(pickCount, data.MaxStack);
                        netItemInstances.Add(new NetItemInstance(null, pick.Value, itemCount));
                        pickCount -= itemCount;
                    }
                }

                foreach (NetItemInstance netItemInstance in netItemInstances)
                {
                    DroppedItem droppedItem = (DroppedItem)_entityManager.Spawn(EntityId.DroppedItem, new SpawnParams() { Position = position });
                    droppedItem.Set(netItemInstance, DroppedItemType.Default);
                }   
            }
            finally
            {
                ListPool<WeightedPick<ItemId>>.Release(picks);
                ListPool<NetItemInstance>.Release(netItemInstances);
            }
        }
    }
}