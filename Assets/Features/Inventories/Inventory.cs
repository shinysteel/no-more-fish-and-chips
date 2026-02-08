using FishFlingers.Items;
using FishFlingers.Networking;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Structures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public string InstanceId { get; private set; }
        public ItemId ItemId { get; private set; }
        public int Count { get; private set; }
        public Vector2Int Pivot { get; private set; }
        public int Rotations { get; private set; }

        public NetInventoryItem(string instanceId, ItemId itemId, int count, Vector2Int pivot, int rotations)
        {
            InstanceId = instanceId;
            ItemId = itemId;
            Count = count;
            Pivot = pivot;
            Rotations = rotations;
        }

        public bool CanAddCount(int amount, out int overflow, out NetInventoryItemsChange change)
        {
            overflow = amount;
            change = default;

            // Guard against invalid adds
            if (amount <= 0)
            {
                return false;
            }

            ItemManager itemManager = GameManager.Instance.Get<ItemManager>();
            ItemData data = itemManager.GetItemData(ItemId);

            // Check for remaining space
            int remainingSpace = data.MaxStack - Count;
            if (remainingSpace == 0)
            {
                return false;
            }

            int changeAmount;

            // Add in mind of remaining space
            if (amount <= remainingSpace)
            {
                changeAmount = amount;
                overflow = 0;
            }
            else
            {
                changeAmount = remainingSpace;
                overflow = amount - remainingSpace;
            }

            change = new NetInventoryItemsChange(InstanceId, changeAmount);

            return true;
        }

        public bool CanRemoveCount(int amount, out int remaining, out NetInventoryItemsChange change)
        {
            if (amount <= 0)
            {
                remaining = amount;
                change = default;
                return false;
            }

            int changeAmount;

            if (Count > amount)
            {
                changeAmount = -amount;
                remaining = 0;
            }
            else
            {
                changeAmount = -Count;
                remaining = amount - Count;
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
            ItemData data = itemManager.GetItemData(ItemId);

            Count = count;
            Count = Mathf.Clamp(Count, 0, data.MaxStack);
        }
    }

    // Instructions to change the count of a NetInventoryItem
    public readonly struct NetInventoryItemsChange
    {
        // Readonly makes the struct immutable internally, and Get; makes the struct immutable externally
        public string InstanceId { get; }
        public int Amount { get; }

        public bool IsValid => InstanceId != null && Amount != 0;

        public NetInventoryItemsChange(string instanceId, int amount)
        {
            InstanceId = instanceId;
            Amount = amount;
        }
    }

    // Instructions to place a NetInventoryItem
    public readonly struct NetInventoryItemsPlace
    {
        public ItemId ItemId { get; }
        public int Amount { get; }
        public Vector2Int Pivot { get; }
        public int Rotations { get; }
        public BoolGrid Shape { get; }

        public bool IsValid => Amount > 0;

        public NetInventoryItemsPlace(ItemId itemId, int amount, Vector2Int pivot, int rotations, BoolGrid shape)
        {
            ItemId = itemId;
            Amount = amount;
            Pivot = pivot;
            Rotations = rotations;
            Shape = shape;
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

            ItemInstance = new ItemInstance(netInventoryItem.InstanceId, data, netInventoryItem.Count);
            Pivot = netInventoryItem.Pivot;
            Rotations = netInventoryItem.Rotations;
            Shape = Rotations == 0 ? ItemInstance.Data.Shape : ItemInstance.Data.Shape.GetRotated(Rotations);
        }
    }

    public class Inventory : GameplayBehaviour, IEnumerable<KeyValuePair<Vector2Int, NetInventorySlot>>
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

        protected override void OnSpawned()
        {
            base.OnSpawned();
            
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

        public NetInventoryItem GetNetInventoryItem(string key)
        {
            return _netInventoryItems[key];
        }

        public void SetLayout(BoolGrid layout)
        {
            _layout = layout;
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
                    _inventoryItems[change.value.InstanceId] = item;

                    OnInventoryItemChanged?.Invoke(change.value.InstanceId, item);
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

        public void RemoveItem(string instanceId)
        { 
            if (!isOwner)
            {
                Log.Error(this, "Tried to remove items without being the owner");
                return;
            }

            if (!_netInventoryItems.ContainsKey(instanceId))
            {
                Log.Error(this, $"Inventory does not contain an item instance with id: {instanceId}");
                return;
            }

            NetInventoryItem item = _netInventoryItems[instanceId];
            ItemData data = _itemManager.GetItemData(item.ItemId);

            // Clear all inventory slots it was on
            foreach (KeyValuePair<Vector2Int, bool> kvp in data.Shape.GetRotated(item.Rotations))
            {
                if (!kvp.Value)
                {
                    continue;
                }

                _netInventorySlots[item.Pivot + kvp.Key].SetItemInstanceId(null);
            }

            _netInventoryItems.Remove(instanceId);
        }

        /// <summary>
        /// Tries to add the given count of an item to the inventory. Will first add to matches,
        /// and then place new instances
        /// </summary>
        public bool TryAddItems(ItemId itemId, int amount)
        {
            if (!isOwner)
            {
                Log.Error(this, "Tried to add items without being the owner");
                return false;
            }

            if (!CanAddItems(itemId, amount, out HashSet<NetInventoryItemsChange> changes, out HashSet<NetInventoryItemsPlace> places))
            {
                return false;
            }

            foreach (NetInventoryItemsChange change in changes)
            {
                ProcessNetInventoryItemsChange(change);
            }

            foreach (NetInventoryItemsPlace place in places)
            {
                ProcessNetInventoryItemsPlace(place, _itemManager.GetNextItemInstanceId());
            }

            return true;
        }

        /// <summary>
        /// Tries to add the given count of an item to a slot. If an item is already there, tries to
        /// add to it. If not, tries to fit by rotating around the pivot
        /// </summary>
        public bool TryPlaceItems(Vector2Int pivot, string instaceId, ItemId itemId, int amount, bool allowPartial, out int overflow)
        {
            overflow = amount;

            if (!isOwner)
            {
                Log.Error(this, "Tried to place items without being the owner");
                return false;
            }

            if (!CanPlaceItems(pivot, itemId, amount, out overflow, out NetInventoryItemsPlace place, out NetInventoryItemsChange change) || overflow > 0)
            {
                if (!allowPartial)
                {
                    return false;
                }
            }

            if (place.IsValid)
            {
                ProcessNetInventoryItemsPlace(place, instaceId);
            }

            if (change.IsValid)
            {
                ProcessNetInventoryItemsChange(change);
            }

            return allowPartial ? overflow < amount : true;
        }

        /// <summary>
        /// Tries to remove the given count of an item from the invenotry
        /// </summary>
        public bool TryRemoveItems(ItemId itemId, int amount)
        {
            if (!isOwner)
            {
                Log.Error(this, "Tried to remove items without being the owner");
                return false;
            }

            if (!CanRemoveItems(itemId, amount, out HashSet<NetInventoryItemsChange> changes))
            {
                return false;
            }

            foreach (NetInventoryItemsChange change in changes)
            {
                ProcessNetInventoryItemsChange(change);
            }

            return true;
        }

        private bool CanAddItems(ItemId itemId, int amount, out HashSet<NetInventoryItemsChange> changes, out HashSet<NetInventoryItemsPlace> places)
        {
            changes = new();
            places = new();

            ItemData data = _itemManager.GetItemData(itemId);

            if (data == null || amount <= 0)
            {
                Log.Error(this, "Checked if invalid items can be added");
                return false;
            }

            int overflow = amount;

            // Check matching instances
            if (data.MaxStack > 1)
            {
                foreach (NetInventoryItem netInventoryItem in _netInventoryItems.Values)
                {
                    if (netInventoryItem.ItemId != itemId)
                    {
                        continue;
                    }

                    if (!netInventoryItem.CanAddCount(overflow, out overflow, out NetInventoryItemsChange change))
                    {
                        continue;
                    }

                    changes.Add(change);

                    if (overflow == 0)
                    {
                        break;
                    }
                }
            }

            HashSet<Vector2Int> placedCells = new();
            void AddPlacedCells(Vector2Int pivot, BoolGrid shape)
            {
                foreach (KeyValuePair<Vector2Int, bool> shapeKvp in shape)
                {
                    if (!shapeKvp.Value)
                    {
                        continue;
                    }

                    placedCells.Add(pivot + shapeKvp.Key);
                }
            }

            // Check empty slots
            if (overflow > 0)
            {
                foreach (KeyValuePair<Vector2Int, NetInventorySlot> slotKvp in _netInventorySlots)
                {
                    if (slotKvp.Value.ItemInstanceId != null)
                    {
                        continue;
                    }

                    if (placedCells.Contains(slotKvp.Key))
                    {
                        continue;
                    }

                    if (!CanPlaceItems(slotKvp.Key, itemId, overflow, out overflow, out NetInventoryItemsPlace place, out NetInventoryItemsChange change))
                    {
                        continue;
                    }

                    if (place.IsValid)
                    {
                        places.Add(place);
                        AddPlacedCells(place.Pivot, place.Shape);
                    }

                    if (change.IsValid)
                    {
                        changes.Add(change);
                    }

                    if (overflow == 0)
                    {
                        break;
                    }
                }
            }

            return overflow == 0;
        }

        // Placing can result in either a place or change, depending on if the pivot is occupied or not
        private bool CanPlaceItems(Vector2Int pivot, ItemId itemId, int amount, out int overflow, out NetInventoryItemsPlace place, out NetInventoryItemsChange change)
        {
            overflow = amount;
            place = default;
            change = default;

            ItemData data = _itemManager.GetItemData(itemId);

            if (data == null || amount <= 0 || amount > data.MaxStack)
            {
                Log.Error(this, "Checked if invalid items can be placed");
                return false;
            }

            // Check if the pivot exists
            if (!_netInventorySlots.TryGetValue(pivot, out NetInventorySlot pivotSlot))
            {
                return false;
            }

            // Check if the pivot is occupied. If so, check if we can add to it
            if (pivotSlot.ItemInstanceId != null)
            {
                NetInventoryItem netInventoryItem = _netInventoryItems[pivotSlot.ItemInstanceId];
                return netInventoryItem.ItemId == itemId && netInventoryItem.CanAddCount(amount, out overflow, out change);
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

            int placeAmount = Mathf.Min(amount, data.MaxStack);

            overflow -= placeAmount;
            place = new NetInventoryItemsPlace(itemId, placeAmount, pivot, rotations, placeShape);

            return true;
        }

        private bool CanRemoveItems(ItemId itemId, int amount, out HashSet<NetInventoryItemsChange> changes)
        {
            changes = new();
            
            ItemData data = _itemManager.GetItemData(itemId);

            if (data == null || amount <= 0)
            {
                Log.Error(this, "Checked if invalid items can be removed");
                return false;
            }

            int remaining = amount;

            foreach (NetInventoryItem netInventoryItem in _netInventoryItems.Values)
            {
                if (netInventoryItem.ItemId != itemId)
                {
                    continue;
                }

                if (!netInventoryItem.CanRemoveCount(remaining, out remaining, out NetInventoryItemsChange change))
                {
                    continue;
                }

                changes.Add(change);

                if (remaining == 0)
                {
                    break;
                }
            }

            return remaining == 0;
        }

        private void ProcessNetInventoryItemsChange(NetInventoryItemsChange change)
        {
            if (!change.IsValid)
            {
                Log.Error(this, "Tried to process an invalid change");
                return;
            }

            NetInventoryItem item = _netInventoryItems[change.InstanceId];
            ItemData data = _itemManager.GetItemData(item.ItemId);

            item.ChangeCount(change.Amount);

            if (item.Count > 0)
            {
                _netInventoryItems.SetDirty(item.InstanceId);
            }
            else
            {
                RemoveItem(item.InstanceId);
            }
        }

        private void ProcessNetInventoryItemsPlace(NetInventoryItemsPlace place, string instanceId)
        {
            if (!place.IsValid)
            {
                Log.Error(this, "Tried to process an invalid place");
                return;
            }

            ItemData data = _itemManager.GetItemData(place.ItemId);

            // Place the items
            NetInventoryItem newNetInventoryItem = new NetInventoryItem(instanceId, place.ItemId, place.Amount, place.Pivot, place.Rotations);
            _netInventoryItems.Add(newNetInventoryItem.InstanceId, newNetInventoryItem);

            foreach (KeyValuePair<Vector2Int, bool> kvp in place.Shape)
            {
                if (!kvp.Value)
                {
                    continue;
                }

                _netInventorySlots[place.Pivot + kvp.Key].SetItemInstanceId(newNetInventoryItem.InstanceId);
            }
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