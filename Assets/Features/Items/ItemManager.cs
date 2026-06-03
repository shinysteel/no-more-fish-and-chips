using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.Networking;
using FishFlingers.Pools;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;
using EntityId = FishFlingers.Entities.EntityId;

namespace FishFlingers.Items
{
    public interface IItemManagerListener
    { }

    public class ItemManager : GameSystem<IItemManagerListener>
    {
        private EntityManager _entityManager;
        private PoolManager _poolManager;

        private ItemManagerConfig _config;

        private Dictionary<ItemId, ItemDefinitionData> _idDataMap = new();
        private Dictionary<ItemId, Pool<ItemModel>> _modelPools = new();

        public override void Initialise(GameManagerConfig config)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _config = config.ItemManagerConfig;

            foreach (ItemDefinitionData data in _config.ItemDataScanner.GetAssets())
            {
                _idDataMap.Add(data.ItemId, data);
            }

            base.Initialise(config);
        }

        public ItemDefinitionData GetItemDefinitionData(ItemId id)
        {
            return _idDataMap[id];
        }

        public IEnumerable<ItemDefinitionData> GetAllItemDefinitionDatas()
        {
            return _idDataMap.Values;
        }

        public void SpawnDrops(Vector3 position, DroppedItemType type, params DropTable[] tables)
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
                    ItemDefinitionData data = GetItemDefinitionData(pick.Value);
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
                    SpawnDroppedItem(netItemInstance, type, position);
                }
            }
            finally
            {
                ListPool<WeightedPick<ItemId>>.Release(picks);
                ListPool<NetItemInstance>.Release(netItemInstances);
            }
        }

        public DroppedItem SpawnDroppedItem(NetItemInstance netItemInstance, DroppedItemType type, Vector3 position)
        {
            DroppedItem droppedItem = (DroppedItem)_entityManager.Spawn(EntityId.DroppedItem, new SpawnParams() { Position = position });
            droppedItem.Set(netItemInstance, type);

            return droppedItem;
        }

        public ItemModel GetModel(ItemId id, SpawnParams parameters)
        {
            return _poolManager.GetPoolable(_modelPools, id, _idDataMap[id].Model, parameters);
        }

        public void ReturnModel(ItemModel model)
        {
            _poolManager.ReturnPoolable(model, model.ItemId, _modelPools);
        }

        public Sprite GetAssignmentSprite(int index)
        {
            return _config.AssignmentSprites[index];
        }
    }
}