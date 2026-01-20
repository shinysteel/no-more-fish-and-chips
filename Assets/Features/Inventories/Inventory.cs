using FishFlingers.Networking;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Structures;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using FishFlingers.Items;
using System;
using System.Linq;
using PrimeTween;

namespace FishFlingers.Inventories
{
    public class NetInventorySlot
    {
        public string ItemInstanceId { get; private set; }

        public void SetItemInstanceId(string id)
        {
            ItemInstanceId = id;
        }
    }

    public class NetInventoryItem
    {
        public string ItemInstanceId { get; private set; }
        public ItemId ItemId { get; private set; }
        public int Count { get; private set; }
        public Vector2Int Pivot { get; private set; }
        public int Rotations { get; private set; }

        public NetInventoryItem(string itemInstanceId, ItemId itemId, int count, Vector2Int pivot, int rotations)
        {
            ItemInstanceId = itemInstanceId;
            ItemId = itemId;
            Count = count;
            Pivot = pivot;
            Rotations = rotations;
        }

        public bool TryAddCount(int amount, out int overflow)
        {
            ItemManager itemManager = GameManager.Instance.Get<ItemManager>();
            ItemData data = itemManager.GetItemData(ItemId);

            // Guard against invalid adds
            if (amount <= 0)
            {
                overflow = amount;
                return false;
            }

            // Check for remaining space
            int remainingSpace = data.MaxStack - Count;
            if (remainingSpace == 0)
            {
                overflow = amount;
                return false;
            }

            // Add in mind of remaining space
            if (amount <= remainingSpace)
            {
                Count += amount;
                overflow = 0;
            }
            else
            {
                Count = data.MaxStack;
                overflow = amount - remainingSpace;
            }

            return true;
        }

        public bool TryRemoveCount(int amount, out int remaining)
        {
            if (amount <= 0)
            {
                remaining = amount;
                return false;
            }

            if (Count > amount)
            {
                Count -= amount;
                remaining = 0;
            }
            else
            {
                remaining = amount - Count;
                Count = 0;
            }

            return true;
        }
    }

    public class InventorySlot
    {
        private Inventory _inventory;
        private string _itemInstanceId;

        public InventoryItem InventoryItem
        {
            get
            {
                return _itemInstanceId != null && _inventory.InventoryItems.TryGetValue(_itemInstanceId, out InventoryItem item)
                    ? item
                    : null;
            }
        }

        public InventorySlot(Inventory inventory, string itemInstanceId)
        {
            _inventory = inventory;
            _itemInstanceId = itemInstanceId;
        }

        public void SetItemInstanceId(string id)
        {
            _itemInstanceId = id;
        }
    }

    public class InventoryItem
    {
        public ItemInstance ItemInstance { get; private set; }
        public Vector2Int Pivot { get; private set; }
        public int Rotations { get; private set; }
        public BoolGrid Shape { get; private set; }

        private ItemManager _itemManager;

        public InventoryItem(NetInventoryItem netInventoryItem)
        {
            _itemManager = GameManager.Instance.Get<ItemManager>();
            ItemData data = _itemManager.GetItemData(netInventoryItem.ItemId);

            ItemInstance = new ItemInstance(netInventoryItem.ItemInstanceId, data, netInventoryItem.Count);
            Pivot = netInventoryItem.Pivot;
            Rotations = netInventoryItem.Rotations;
            Shape = Rotations == 0 ? ItemInstance.Data.Shape : ItemInstance.Data.Shape.GetRotated(Rotations);
        }
    }

    public class Inventory : NetBehaviour, IEnumerable<KeyValuePair<Vector2Int, NetInventorySlot>>
    {
        private SyncDictionaryWrapper<Vector2Int, NetInventorySlot> _netInventorySlots = new(ownerAuth: true);
        private SyncDictionaryWrapper<string, NetInventoryItem> _netInventoryItems = new(ownerAuth: true);

        private Dictionary<Vector2Int, InventorySlot> _inventorySlots = new();
        private Dictionary<string, InventoryItem> _inventoryItems = new();

        public IReadOnlyDictionary<Vector2Int, InventorySlot> InventorySlots => _inventorySlots;
        public IReadOnlyDictionary<string, InventoryItem> InventoryItems => _inventoryItems;

        private BoolGrid _layout;

        public int Columns => _layout.Columns;
        public int Rows => _layout.Rows;

        // It was not obvious that the string in Action<string, InventoryItem> represented instanceId. This
        // is a good example of when to use custom delegates. If more parameters could be added in the future,
        // then you could also consider using EventArgs
        public delegate void InventoryItemChangedDelegate(string instanceId, InventoryItem inventoryItem);
        public event InventoryItemChangedDelegate OnInventoryItemChanged;

        public void Initialise(BoolGrid layout)
        {
            _layout = layout;
        }

        protected override void OnSpawned()
        {
            _netInventorySlots.onChanged += HandleNetInventorySlotsChanged;
            _netInventoryItems.onChanged += HandleNetInventoryItemsChanged;

            if (!isOwner)
            {
                return;
            }

            PopulateSlots();            
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            if (_netInventorySlots != null)
            {
                _netInventorySlots.onChanged -= HandleNetInventorySlotsChanged;
            }

            if (_netInventoryItems != null)
            {
                _netInventoryItems.onChanged -= HandleNetInventoryItemsChanged;
            }
        }

        private void PopulateSlots()
        {
            foreach (KeyValuePair<Vector2Int, bool> kvp in _layout)
            {
                if (kvp.Value == true)
                {
                    _netInventorySlots.Add(kvp.Key, new());
                }
            }
        }

        private void HandleNetInventorySlotsChanged(SyncDictionaryChange<Vector2Int, NetInventorySlot> change)
        {
            switch (change.operation)
            {
                case SyncDictionaryOperation.Added:
                    InventorySlot slot = new InventorySlot(this, change.value.ItemInstanceId);
                    _inventorySlots.Add(change.key, slot);
                    break;

                case SyncDictionaryOperation.Removed:
                    _inventorySlots.Remove(change.key);
                    break;

                case SyncDictionaryOperation.Set:
                    _inventorySlots[change.key].SetItemInstanceId(change.value.ItemInstanceId);
                    break;

                case SyncDictionaryOperation.Cleared:
                    _inventorySlots.Clear();
                    break;
            }
        }

        private void HandleNetInventoryItemsChanged(SyncDictionaryChange<string, NetInventoryItem> change)
        {
            switch (change.operation)
            {
                case SyncDictionaryOperation.Added:
                case SyncDictionaryOperation.Set:
                    InventoryItem item = new InventoryItem(change.value);
                    _inventoryItems[change.value.ItemInstanceId] = item;

                    OnInventoryItemChanged?.Invoke(change.value.ItemInstanceId, item);
                    break;

                case SyncDictionaryOperation.Removed:
                    string instanceId = _inventoryItems[change.key].ItemInstance.InstanceId;
                    _inventoryItems.Remove(change.key);

                    OnInventoryItemChanged?.Invoke(instanceId, null);
                    break;

                case SyncDictionaryOperation.Cleared:
                    string[] instanceIds = _inventoryItems.Keys.ToArray();
                    _inventoryItems.Clear();

                    foreach (string id in instanceIds)
                    {
                        OnInventoryItemChanged?.Invoke(id, null);
                    }
                    break;
            }
        }

        /// <summary>
        /// Tries to add the given count of an item to the inventory. Will first add to matches,
        /// and then place new instances
        /// </summary>
        /// <param name="itemId">The item's id</param>
        /// <param name="amount">The amount to add</param>
        /// <param name="overflow">The remaining amount</param>
        /// <returns>True if any was added, false if none was added</returns>
        public bool TryAddItems(ItemId itemId, int amount, out int overflow)
        {
            overflow = amount;

            if (!isOwner)
            {
                Debugger.LogError(this, "Tried to add items without being the owner");
                return false;
            }

            ItemData data = _itemManager.GetItemData(itemId);

            if (data == null || amount <= 0)
            {
                Debugger.LogError(this, "Tried to add invalid items");
                return false;
            }

            // Add to matching instances
            if (data.MaxStack > 1)
            {
                foreach (NetInventoryItem netInventoryItem in _netInventoryItems.Values)
                {
                    if (netInventoryItem.ItemId != itemId)
                    {
                        continue;
                    }

                    if (!netInventoryItem.TryAddCount(overflow, out overflow))
                    {
                        continue;
                    }

                    _netInventoryItems.SetDirty(netInventoryItem.ItemInstanceId);

                    if (overflow == 0)
                    {
                        return true;
                    }                    
                }
            }

            // Add to empty slots
            foreach (KeyValuePair<Vector2Int, NetInventorySlot> kvp in _netInventorySlots)
            {
                if (kvp.Value.ItemInstanceId != null)
                {
                    continue;
                }

                if (TryPlaceItems(kvp.Key, itemId, overflow, out overflow) && overflow == 0)
                {
                    return true;
                }
            }

            return overflow < amount;
        }

        /// <summary>
        /// Tries to add the given count of an item to a slot. If an item is already there, tries to
        /// add to it. If not, tries to fit by rotating around the pivot
        /// </summary>
        /// <param name="pivot">The cell to rotate around</param>
        /// <param name="itemId">The item's id</param>
        /// <param name="amount">The amount to add</param>
        /// <param name="overflow">The remaining amount</param>
        /// <returns>True if any was added, false if none was added</returns>
        public bool TryPlaceItems(Vector2Int pivot, ItemId itemId, int amount, out int overflow)
        {
            overflow = amount;

            if (!isOwner)
            {
                Debugger.LogError(this, "Tried to place items without being the owner");
                return false;
            }

            ItemData data = _itemManager.GetItemData(itemId);

            if (data == null || amount <= 0)
            {
                Debugger.LogError(this, "Tried to place invalid items");
                return false;
            }

            // Early check if the pivot exists
            if (!_netInventorySlots.TryGetValue(pivot, out NetInventorySlot pivotSlot))
            {
                return false;
            }

            // If an instance is already occupying the pivot, try add to it
            if (pivotSlot.ItemInstanceId != null)
            {
                NetInventoryItem netInventoryItem = _netInventoryItems[pivotSlot.ItemInstanceId];

                if (netInventoryItem.ItemId == itemId)
                {
                    bool result = netInventoryItem.TryAddCount(amount, out overflow);
                    _netInventoryItems.SetDirty(pivotSlot.ItemInstanceId);
                    return result;
                }
                else
                {
                    return false;
                }
            }

            BoolGrid placeShape = null;
            int rotations;

            // Test all cell and rotation combinations for a fit
            for (rotations = 0; rotations < 4; rotations++)
            {
                BoolGrid shape = rotations == 0 ? data.Shape : data.Shape.GetRotated(rotations);
                bool fits = true;

                foreach (KeyValuePair<Vector2Int, bool> kvp in shape)
                {
                    if (!_netInventorySlots.TryGetValue(pivot + kvp.Key, out NetInventorySlot slot))
                    {
                        fits = false;
                        break;
                    }

                    if (slot.ItemInstanceId != null)
                    { 
                        fits = false;
                        break;
                    }
                }

                if (fits)
                {
                    placeShape = shape;
                    break;
                }
            }

            if (placeShape == null)
            {
                return false;
            }

            // Place the items
            int count = Mathf.Min(amount, data.MaxStack);
            NetInventoryItem newNetInventoryItem = new NetInventoryItem(_itemManager.GetNextItemInstanceId(), itemId, count, pivot, rotations);
            _netInventoryItems.Add(newNetInventoryItem.ItemInstanceId, newNetInventoryItem);

            foreach (KeyValuePair<Vector2Int, bool> kvp in placeShape)
            {
                if (!kvp.Value)
                {
                    continue;
                }

                _netInventorySlots[pivot + kvp.Key].SetItemInstanceId(newNetInventoryItem.ItemInstanceId);
            }

            overflow = amount - count;
            return true;            
        }

        public bool TryRemoveItems(ItemId itemId, int amount, out int remaining)
        {
            remaining = amount;

            ItemData data = _itemManager.GetItemData(itemId);

            if (data == null || amount <= 0)
            {
                Debugger.LogError(this, "Tried to remove invalid items");
                return false;
            }

            // You can't modify a collection while enumerating it - use .ToArray
            foreach (NetInventoryItem netInventoryItem in _netInventoryItems.Values.ToArray())
            {
                if (netInventoryItem.ItemId != itemId)
                {
                    continue;
                }

                if (!netInventoryItem.TryRemoveCount(remaining, out remaining))
                {
                    continue;
                }
                
                if (netInventoryItem.Count > 0)
                {
                    _netInventoryItems.SetDirty(netInventoryItem.ItemInstanceId);
                }
                else
                {
                    // Clear all inventory slots it was on
                    foreach (KeyValuePair<Vector2Int, bool> kvp in data.Shape.GetRotated(netInventoryItem.Rotations))
                    {
                        if (!kvp.Value)
                        {
                            continue;
                        }

                        _netInventorySlots[netInventoryItem.Pivot + kvp.Key].SetItemInstanceId(null);
                    }

                    _netInventoryItems.Remove(netInventoryItem.ItemInstanceId);
                }

                if (remaining == 0)
                {
                    return true;
                }
            }

            return remaining < amount;
        }

        public IEnumerator<KeyValuePair<Vector2Int, NetInventorySlot>> GetEnumerator()
        {
            return _netInventorySlots.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}