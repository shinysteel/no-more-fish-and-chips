using UnityEngine;
using FishFlingers.Inventories;
using FishFlingers.Networking;
using FishFlingers.Entities;

namespace FishFlingers.Items
{
    public class ItemInstance : IDeepCloneable<ItemInstance>
    {
        public string InstanceId { get; private set; }
        public ItemDefinitionData Data { get; private set; }
        public int Count { get; private set; }

        public ItemInstance(string instanceId, ItemDefinitionData data, int count)
        {
            InstanceId = instanceId;
            Data = data;
            Count = Mathf.Clamp(count, 0, data.MaxStack);
        }

        public static ItemInstance Create(NetItemInstance netItemInstance)
        {
            ItemManager itemManager = GameManager.Instance.Get<ItemManager>();
            return new ItemInstance(netItemInstance.InstanceId, itemManager.GetItemDefinitionData(netItemInstance.ItemId), netItemInstance.Count);
        }

        public ItemInstance DeepClone()
        {
            return new ItemInstance(InstanceId, Data, Count);
        }
    }

    public class NetItemInstance : IDeepCloneable<NetItemInstance>
    {
        public string InstanceId { get; private set; }
        public ItemId ItemId { get; private set; }
        public int Count { get; private set; }

        public NetItemInstance(string instanceId, ItemId itemId, int count)
        {
            InstanceId = instanceId;
            ItemId = itemId;
            SetCount(count);
        }

        public static NetItemInstance Create(ItemInstance itemInstance)
        {
            return new NetItemInstance(itemInstance.InstanceId, itemInstance.Data.ItemId, itemInstance.Count);
        }

        public static NetItemInstance Create(DroppedItemSave droppedItemSave)
        {
            return new NetItemInstance(droppedItemSave.InstanceId, droppedItemSave.ItemId, droppedItemSave.Count);
        }

        public NetItemInstance DeepClone()
        {
            return new NetItemInstance(InstanceId, ItemId, Count);
        }

        public bool CanAddCount(int count, out int overflow, out NetInventoryItemsChange change)
        {
            overflow = count;
            change = default;

            // Guard against invalid adds
            if (count <= 0)
            {
                return false;
            }

            ItemManager itemManager = GameManager.Instance.Get<ItemManager>();
            ItemDefinitionData data = itemManager.GetItemDefinitionData(ItemId);

            // Check for remaining space
            int remainingSpace = data.MaxStack - Count;
            if (remainingSpace == 0)
            {
                return false;
            }

            int changeAmount;

            // Add in mind of remaining space
            if (count <= remainingSpace)
            {
                changeAmount = count;
                overflow = 0;
            }
            else
            {
                changeAmount = remainingSpace;
                overflow = count - remainingSpace;
            }

            change = new NetInventoryItemsChange(InstanceId, changeAmount);

            return true;
        }

        public bool CanRemoveCount(int count, out int remaining, out NetInventoryItemsChange change)
        {
            if (count <= 0)
            {
                remaining = count;
                change = default;
                return false;
            }

            int changeAmount;

            if (Count > count)
            {
                changeAmount = -count;
                remaining = 0;
            }
            else
            {
                changeAmount = -Count;
                remaining = count - Count;
            }

            change = new NetInventoryItemsChange(InstanceId, changeAmount);

            return true;
        }

        public void ChangeCount(int amount)
        {
            SetCount(Count + amount);
        }

        public void SetCount(int count)
        {
            ItemManager itemManager = GameManager.Instance.Get<ItemManager>();
            ItemDefinitionData data = itemManager.GetItemDefinitionData(ItemId);

            count = Mathf.Clamp(count, 0, data.MaxStack);
            Count = count;
        }
    }
}