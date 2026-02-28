using FishFlingers.Items;
using FishFlingers.Networking;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Structures;
using ShinyOwl.Common.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace FishFlingers.Inventories
{
    public class PlaceParams
    {
        public Vector2Int Cell { get; set; } = Vector2Int.zero;
        public Vector2Int Pivot { get; set; } = Vector2Int.zero;
        public RotationParams RotationParams { get; set; } = new();
        public string InstanceId { get; set; } = null;
        public ItemId ItemId { get; set; } = default;
        public int Amount { get; set; } = 0;

        public static PlaceParams Create(Vector2Int cell, InventoryItem item)
        {
            return new PlaceParams()
            {
                Cell = cell,
                Pivot = item.Pivot,
                RotationParams = new RotationParams() { Rotations = item.Rotations },
                InstanceId = item.ItemInstance.InstanceId,
                ItemId = item.ItemInstance.Data.ItemId,
                Amount = item.ItemInstance.Count
            };
        }
    }

    public class RotationParams
    {
        public int Rotations { get; set; } = 0;
        public bool AutoFit { get; set; } = false;
    }

    public class NetInventorySlot
    {
        public string ItemInstanceId { get; private set; }

        public void SetItemInstanceId(string id)
        {
            ItemInstanceId = id;
        }
    }

    public class NetInventoryItem : IDeepCloneable<NetInventoryItem>
    {
        public Vector2Int Cell { get; private set; }
        public Vector2Int Pivot { get; private set; }
        public int Rotations { get; private set; }
        public string InstanceId { get; private set; }
        public ItemId ItemId { get; private set; }
        public int Count { get; private set; }

        public NetInventoryItem(Vector2Int cell, Vector2Int pivot, int rotations, string instanceId, ItemId itemId, int count)
        {
            Cell = cell;
            SetPivot(pivot);
            Rotations = rotations;
            InstanceId = instanceId;
            ItemId = itemId;
            SetCount(count);
        }

        public NetInventoryItem DeepClone()
        {
            return new NetInventoryItem(Cell, Pivot, Rotations, InstanceId, ItemId, Count);
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

            count = Mathf.Clamp(count, 0, data.MaxStack);
            Count = count;
        }

        public void SetPivot(Vector2Int pivot)
        {
            Pivot = pivot;
        }

        public void ChangeRotations(int amount)
        {
            SetRotations(Rotations + amount);
        }

        private void SetRotations(int rotations)
        {
            rotations = Utils.Math.EuclideanModulo(rotations, 4);
            Rotations = rotations;
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
        public PlaceParams Parameters { get; }
        public BoolGrid Shape { get; }

        public bool IsValid => Parameters?.Amount > 0;

        public NetInventoryItemsPlace(PlaceParams parameters, BoolGrid shape)
        {
            Parameters = parameters;
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
        public Vector2Int Cell { get; private set; }
        public Vector2Int Pivot { get; private set; }
        public int Rotations { get; private set; }
        public ItemInstance ItemInstance { get; private set; }
        public BoolGrid Shape { get; private set; }

        private ItemManager _itemManager;

        public InventoryItem(NetInventoryItem netInventoryItem)
        {
            _itemManager = GameManager.Instance.Get<ItemManager>();
            ItemData data = _itemManager.GetItemData(netInventoryItem.ItemId);

            Cell = netInventoryItem.Cell;
            Pivot = netInventoryItem.Pivot;
            Rotations = netInventoryItem.Rotations;
            ItemInstance = new ItemInstance(netInventoryItem.InstanceId, data, netInventoryItem.Count);
            Shape = ItemInstance.Data.Shape.GetTransformed(Pivot, Rotations);
        }
    }

    public class Inventory : GameplayBehaviour, IEnumerable<KeyValuePair<Vector2Int, NetInventorySlot>>
    {
        private SyncDictionaryWrapper<Vector2Int, NetInventorySlot> _netInventorySlots = new(ownerAuth: true);
        private SyncDictionaryWrapper<string, NetInventoryItem> _netInventoryItems = new(ownerAuth: true);

        public SyncDictionaryWrapper<Vector2Int, NetInventorySlot> NetInventorySlots => _netInventorySlots;
        public SyncDictionaryWrapper<string, NetInventoryItem> NetInventoryItems => _netInventoryItems;

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
        public delegate void InventoryItemChangedDelegate(string instanceId, InventoryItem oldInventoryItem, InventoryItem newInventoryItem);
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

        public void SetLayout(BoolGrid layout)
        {
            _layout = layout;
        }

        private void PopulateSlots()
        {
            _layout.ForEachTrue((Vector2Int cell) =>
            {
                _netInventorySlots.Add(cell, new());
            });
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
            // You can't reuse a variable name between cases
            InventoryItem oldInventoryItem;

            switch (change.operation)
            {
                case SyncDictionaryOperation.Added:
                case SyncDictionaryOperation.Set:
                    _inventoryItems.TryGetValue(change.value.InstanceId, out oldInventoryItem);
                    InventoryItem newInventoryItem = new InventoryItem(change.value);

                    _inventoryItems[change.value.InstanceId] = newInventoryItem;

                    OnInventoryItemChanged?.Invoke(change.value.InstanceId, oldInventoryItem, newInventoryItem);
                    break;

                case SyncDictionaryOperation.Removed:
                    oldInventoryItem = _inventoryItems[change.key];
                    string instanceId = oldInventoryItem.ItemInstance.InstanceId;

                    _inventoryItems.Remove(change.key);

                    OnInventoryItemChanged?.Invoke(instanceId, oldInventoryItem, null);
                    break;

                case SyncDictionaryOperation.Cleared:
                    string[] instanceIds = _inventoryItems.Keys.ToArray();
                    _inventoryItems.Clear();

                    foreach (string id in instanceIds)
                    {
                        oldInventoryItem = _inventoryItems[id];
                        OnInventoryItemChanged?.Invoke(id, oldInventoryItem, null);
                    }
                    break;
            }
        }

        /// <summary>
        /// Removes an item from the inventory
        /// </summary>
        public void RemoveItem(string instanceId)
        { 
            if (!isOwner)
            {
                Log.Error("Tried to remove items without being the owner");
                return;
            }

            if (!_netInventoryItems.ContainsKey(instanceId))
            {
                Log.Error($"Inventory does not contain an item instance with id: {instanceId}");
                return;
            }

            ClearSlots(instanceId);
            _netInventoryItems.Remove(instanceId);
        }

        /// <summary>
        /// Clear all inventory slots an item is on. This won't actually remove the item from the inventory, and
        /// is useful for moving an existing item without removing it first
        /// </summary>
        private void ClearSlots(string instanceId)
        {
            if (!isOwner)
            {
                Log.Error("Tried to clear slots without being the owner");
                return;
            }

            if (!_netInventoryItems.TryGetValue(instanceId, out NetInventoryItem item))
            {
                Log.Error($"Inventory does not contain an item instance with id: {instanceId}");
                return;
            }

            ItemData data = _itemManager.GetItemData(item.ItemId);

            data.Shape.GetTransformed(item.Pivot, item.Rotations).ForEachTrue((Vector2Int cell) =>
            {
                _netInventorySlots[item.Cell + cell].SetItemInstanceId(null);
            });
        }

        /// <summary>
        /// Tries to add the given count of an item to the inventory. Will first add to matches,
        /// and then place new instances
        /// </summary>
        public bool TryAddItems(ItemId itemId, int amount)
        {
            if (!isOwner)
            {
                Log.Error("Tried to add items without being the owner");
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
                ProcessNetInventoryItemsPlace(place);
            }

            return true;
        }

        /// <summary>
        /// Tries to add the given count of an item to a slot. If an item is already there, tries to
        /// add to it. If not, tries to fit by rotating around the pivot
        /// </summary>
        public bool TryPlaceItems(PlaceParams parameters, bool allowPartial, out int overflow)
        {
            overflow = parameters.Amount;

            if (!isOwner)
            {
                Log.Error("Tried to place items without being the owner");
                return false;
            }

            if (!CanPlaceItems(parameters, out overflow, out NetInventoryItemsPlace place, out NetInventoryItemsChange change) || overflow > 0)
            {
                if (!allowPartial)
                {
                    return false;
                }
            }

            if (place.IsValid)
            {
                ProcessNetInventoryItemsPlace(place);
            }

            if (change.IsValid)
            {
                ProcessNetInventoryItemsChange(change);
            }

            return allowPartial ? overflow < parameters.Amount : true;
        }

        /// <summary>
        /// Tries to remove the given count of an item from the invenotry
        /// </summary>
        public bool TryRemoveItems(ItemId itemId, int amount)
        {
            if (!isOwner)
            {
                Log.Error("Tried to remove items without being the owner");
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
                Log.Error("Checked if invalid items can be added");
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
            void AddPlacedCells(Vector2Int placedCell, BoolGrid shape)
            {
                shape.ForEachTrue((Vector2Int shapeCell) =>
                {
                    placedCells.Add(placedCell + shapeCell);
                });
            }

            // Check empty slots
            if (overflow > 0)
            {
                foreach (KeyValuePair<Vector2Int, NetInventorySlot> kvp in _netInventorySlots)
                {
                    if (kvp.Value.ItemInstanceId != null)
                    {
                        continue;
                    }

                    if (placedCells.Contains(kvp.Key))
                    {
                        continue;
                    }

                    PlaceParams parameters = new PlaceParams()
                    {
                        Cell = kvp.Key,
                        RotationParams = new RotationParams() { AutoFit = true },
                        ItemId = itemId,
                        Amount = overflow
                    };

                    if (!CanPlaceItems(parameters, out overflow, out NetInventoryItemsPlace place, out NetInventoryItemsChange change))
                    {
                        continue;
                    }

                    if (place.IsValid)
                    {
                        places.Add(place);
                        AddPlacedCells(place.Parameters.Cell, place.Shape);
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

        // Placing can result in either a place or change, depending on if the cell is occupied or not
        public bool CanPlaceItems(PlaceParams parameters, out int overflow, out NetInventoryItemsPlace place, out NetInventoryItemsChange change)
        {
            overflow = parameters.Amount;
            place = default;
            change = default;

            ItemData data = _itemManager.GetItemData(parameters.ItemId);

            if (data == null || parameters.Amount <= 0 || parameters.Amount > data.MaxStack)
            {
                Log.Error("Checked if invalid items can be placed");
                return false;
            }

            // Check if the cell exists
            if (!_netInventorySlots.TryGetValue(parameters.Cell, out NetInventorySlot netInventorySlot))
            {
                return false;
            }

            // Check if the cell is occupied. If so, check if we can add to it
            if (netInventorySlot.ItemInstanceId != null && netInventorySlot.ItemInstanceId != parameters.InstanceId)
            {
                NetInventoryItem netInventoryItem = _netInventoryItems[netInventorySlot.ItemInstanceId];
                return netInventoryItem.ItemId == parameters.ItemId && netInventoryItem.CanAddCount(parameters.Amount, out overflow, out change);
            }

            BoolGrid placeShape = null;
            int rotations;

            // Test cell and rotation combinations for a fit
            for (rotations = parameters.RotationParams.Rotations; rotations < parameters.RotationParams.Rotations + (parameters.RotationParams.AutoFit ? 4 : 1); rotations++)
            {
                BoolGrid shape = data.Shape.GetTransformed(parameters.Pivot, rotations);
                bool fits = true;

                shape.ForEachTrue((Vector2Int shapeCell) =>
                {
                    if (!fits)
                    {
                        return;
                    }
                    
                    if (!_netInventorySlots.TryGetValue(parameters.Cell + shapeCell, out NetInventorySlot slot))
                    {
                        fits = false;
                        return;
                    }

                    // It's okay to match our instanceId, since it would then be moving the item
                    if (slot.ItemInstanceId != null && slot.ItemInstanceId != parameters.InstanceId)
                    {
                        fits = false;
                        return;
                    }
                });

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

            parameters.RotationParams = new RotationParams() { Rotations = rotations };

            parameters.Amount = Mathf.Min(parameters.Amount, data.MaxStack);

            overflow -= parameters.Amount;
            place = new NetInventoryItemsPlace(parameters, placeShape);

            return true;
        }

        private bool CanRemoveItems(ItemId itemId, int amount, out HashSet<NetInventoryItemsChange> changes)
        {
            changes = new();
            
            ItemData data = _itemManager.GetItemData(itemId);

            if (data == null || amount <= 0)
            {
                Log.Error("Checked if invalid items can be removed");
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
                Log.Error("Tried to process an invalid change");
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

        private void ProcessNetInventoryItemsPlace(NetInventoryItemsPlace place)
        {
            if (!place.IsValid)
            {
                Log.Error("Tried to process an invalid place");
                return;
            }

            ItemData data = _itemManager.GetItemData(place.Parameters.ItemId);

            // A null place.InstanceId indicates this will be a new item. We validate instanceId here since we know for sure it will be placed
            string instanceId = place.Parameters.InstanceId != null ? place.Parameters.InstanceId : _itemManager.GetNextItemInstanceId();

            NetInventoryItem item = new NetInventoryItem(place.Parameters.Cell, place.Parameters.Pivot, place.Parameters.RotationParams.Rotations, instanceId, place.Parameters.ItemId, place.Parameters.Amount);

            if (!_netInventoryItems.ContainsKey(instanceId))
            {
                _netInventoryItems.Add(instanceId, item);
            }
            else
            {
                // If we are moving an item, we need to clear its previous placement
                ClearSlots(instanceId);

                _netInventoryItems[instanceId] = item;
            }

            // Place the items
            place.Shape.ForEachTrue((Vector2Int cell) =>
            {
                _netInventorySlots[place.Parameters.Cell + cell].SetItemInstanceId(instanceId);
            });
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