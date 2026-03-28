using FishFlingers.Entities;
using FishFlingers.Items;
using FishFlingers.Networking;
using Newtonsoft.Json;
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
    public static class InventoryItemUtils
    {
        /// <summary>
        /// Before making rotations relative to a new cell, the pivot needs to be recalculated
        /// </summary>
        public static Vector2Int RecalculatePivot(Vector2Int oldCell, Vector2Int newCell, Vector2Int pivot, int rotations)
        {
            Vector2Int origin = oldCell - Utils.Math.RotateCell(pivot, rotations, true);
            Vector2Int offset = newCell - origin;
            return Utils.Math.RotateCell(offset, rotations, false);
        }
    }

    public class InventorySave
    {
        [JsonProperty] public List<InventoryItemSave> Items { get; private set; } = new();
    }

    public class InventoryItemSave
    {
        [JsonProperty] private SimpleVector2Int _cell = new();
        [JsonProperty] private SimpleVector2Int _pivot = new();

        [JsonIgnore]
        public Vector2Int Cell
        {
            get => _cell.ToVector2Int();
            set => _cell = new SimpleVector2Int(value);
        }

        [JsonIgnore]
        public Vector2Int Pivot
        {
            get => _pivot.ToVector2Int();
            set => _pivot = new SimpleVector2Int(value);
        }

        [JsonProperty] public int Rotations { get; private set; }
        [JsonProperty] public string InstanceId { get; private set; }
        [JsonProperty] public ItemId ItemId { get; private set; }
        [JsonProperty] public int Count { get; private set; }

        public InventoryItemSave()
        { }
        
        public InventoryItemSave(InventoryItem item) : this(item.Cell, item.Pivot, item.Rotations, item.ItemInstance.InstanceId, item.ItemInstance.Data.ItemId, item.ItemInstance.Count)
        { }

        public InventoryItemSave(Vector2Int cell, Vector2Int pivot, int rotations, string instanceId, ItemId itemId, int count)
        {
            Cell = cell;
            Pivot = pivot;
            Rotations = rotations;
            InstanceId = instanceId;
            ItemId = itemId;
            Count = count;
        }
    }
    
    public class InventoryChangeParams
    {
        public string InstanceId { get; set; } = null;
        public ItemId ItemId { get; set; } = default;
        public int Count { get; set; } = 0;
    }
   
    public class InventoryPlaceParams
    {
        public Vector2Int Cell { get; set; } = Vector2Int.zero;
        public Vector2Int Pivot { get; set; } = Vector2Int.zero;
        public InventoryRotationParams RotationParams { get; set; } = new();
        public string InstanceId { get; set; } = null;
        public ItemId ItemId { get; set; } = default;
        public int Count { get; set; } = 0;

        public static InventoryPlaceParams Create(Vector2Int cell, InventoryItem item)
        {
            return new InventoryPlaceParams()
            {
                Cell = cell,
                Pivot = item.Pivot,
                RotationParams = new InventoryRotationParams() { Rotations = item.Rotations },
                InstanceId = item.ItemInstance.InstanceId,
                ItemId = item.ItemInstance.Data.ItemId,
                Count = item.ItemInstance.Count
            };
        }

        public static InventoryPlaceParams Create(InventoryItemSave save)
        {
            return new InventoryPlaceParams()
            {
                Cell = save.Cell,
                Pivot = save.Pivot,
                RotationParams = new InventoryRotationParams() { Rotations = save.Rotations },
                InstanceId = save.InstanceId,
                ItemId = save.ItemId,
                Count = save.Count
            };
        }
    }

    public class InventoryRotationParams
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
            ItemData data = itemManager.GetItemData(ItemId);

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
        public InventoryPlaceParams Parameters { get; }
        public BoolGrid Shape { get; }

        public bool IsValid => Parameters?.Count > 0;

        public NetInventoryItemsPlace(InventoryPlaceParams parameters, BoolGrid shape)
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

    public class InventoryItem : IDeepCloneable<InventoryItem>
    {
        public Vector2Int Cell { get; private set; }
        public Vector2Int Pivot { get; private set; }
        public int Rotations { get; private set; }
        public ItemInstance ItemInstance { get; private set; }
        public BoolGrid Shape { get; private set; }

        private InventoryItem(Vector2Int cell, Vector2Int pivot, int rotations, ItemInstance instance)
        {
            Cell = cell;
            Pivot = pivot;
            Rotations = rotations;
            ItemInstance = instance;

            RefreshShape();
        }

        public static InventoryItem Create(NetInventoryItem item)
        {
            ItemManager itemManager = GameManager.Instance.Get<ItemManager>();
            ItemData data = itemManager.GetItemData(item.ItemId);
            ItemInstance instance = new ItemInstance(item.InstanceId, data, item.Count);

            return new InventoryItem(item.Cell, item.Pivot, item.Rotations, instance);
        }

        public InventoryItem DeepClone()
        {
            return new InventoryItem(Cell, Pivot, Rotations, ItemInstance.DeepClone());
        }
        
        public void SetPivot(Vector2Int pivot)
        {
            if (Pivot == pivot)
            {
                return;
            }
            
            Pivot = pivot;

            RefreshShape();
        }

        public void ChangeRotations(int amount)
        {
            if (amount == 0)
            {
                return;
            }

            Rotations += amount;
            Rotations = Utils.Math.EuclideanModulo(Rotations, 4);

            RefreshShape();
        }

        private void RefreshShape()
        {
            Shape = ItemInstance.Data.Shape.GetTransformed(Pivot, Rotations);
        }
    }

    public class Inventory : GameplayBehaviour, IEnumerable<KeyValuePair<Vector2Int, NetInventorySlot>>
    {
        private SyncDictionaryWrapper<Vector2Int, NetInventorySlot> _netInventorySlots = new SyncDictionaryWrapper<Vector2Int, NetInventorySlot>(ownerAuth: true);
        private SyncDictionaryWrapper<string, NetInventoryItem> _netInventoryItems = new SyncDictionaryWrapper<string, NetInventoryItem>(ownerAuth: true);

        public SyncDictionaryWrapper<Vector2Int, NetInventorySlot> NetInventorySlots => _netInventorySlots;
        public SyncDictionaryWrapper<string, NetInventoryItem> NetInventoryItems => _netInventoryItems;

        private Dictionary<Vector2Int, InventorySlot> _inventorySlots = new();
        private Dictionary<string, InventoryItem> _inventoryItems = new();

        public IReadOnlyDictionary<Vector2Int, InventorySlot> InventorySlots => _inventorySlots;
        public IReadOnlyDictionary<string, InventoryItem> InventoryItems => _inventoryItems;

        private BoolGrid _layout;

        public int Columns => _layout.Columns;
        public int Rows => _layout.Rows;

        public bool IsReady => _netInventorySlots.IsReady && _netInventoryItems.IsReady;

        // It was not obvious that the string in Action<string, InventoryItem> represented instanceId. This
        // is a good example of when to use custom delegates. If more parameters could be added in the future,
        // then you could also consider using EventArgs
        public delegate void InventoryItemChangedDelegate(string instanceId, InventoryItem oldInventoryItem, InventoryItem newInventoryItem);
        public event InventoryItemChangedDelegate OnInventoryItemChanged;

        protected override void OnSpawned()
        {
            base.OnSpawned();
          
            foreach (KeyValuePair<Vector2Int, NetInventorySlot> kvp in _netInventorySlots)
            {
                HandleNetInventorySlotsChanged(new SyncDictionaryChange<Vector2Int, NetInventorySlot>()
                {
                    operation = SyncDictionaryOperation.Added,
                    key = kvp.Key,
                    value = kvp.Value
                });
            }

            _netInventorySlots.onChanged += HandleNetInventorySlotsChanged;

            foreach (KeyValuePair<string, NetInventoryItem> kvp in _netInventoryItems)
            {
                HandleNetInventoryItemsChanged(new SyncDictionaryChange<string, NetInventoryItem>()
                {
                    operation = SyncDictionaryOperation.Added,
                    key = kvp.Key,
                    value = kvp.Value
                });
            }

            _netInventoryItems.onChanged += HandleNetInventoryItemsChanged;

            if (isOwner)
            {
                PopulateSlots();
            }
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _netInventorySlots.onChanged -= HandleNetInventorySlotsChanged;
            _netInventoryItems.onChanged -= HandleNetInventoryItemsChanged;            
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
                    InventoryItem newInventoryItem = InventoryItem.Create(change.value);

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
        /// Tries to remove a collection of items. Fails if the request can't be fulfilled entirely
        /// </summary>
        public bool TryRemoveItems(List<InventoryChangeParams> allParameters)
        {
            if (!isOwner)
            {
                Log.Error($"Tried to remove items without being the owner");
                return false;
            }

            if (!CanRemoveItems(allParameters, out List<NetInventoryItemsChange> allChanges))
            {
                return false;
            }

            foreach (NetInventoryItemsChange change in allChanges)
            {
                ProcessNetInventoryItemsChange(change);
            }

            return true;
        }

        public bool CanRemoveItems(List<InventoryChangeParams> allParameters, out List<NetInventoryItemsChange> allChanges)
        {
            allChanges = new();

            bool canRemove = true;

            foreach (InventoryChangeParams parameters in allParameters)
            {
                if (!CanRemoveItem(parameters, out _, out List<NetInventoryItemsChange> changes))
                {
                    canRemove = false;
                    break;
                }

                allChanges.AddRange(changes);
            }

            return canRemove;
        }

        /// <summary>
        /// Tries to add the given count of an item to the inventory. Will first add to matches,
        /// and then place new instances
        /// </summary>
        public bool TryAddItem(InventoryChangeParams parameters, bool allowPartial, out int overflow, out List<NetInventoryItemsChange> changes, out List<NetInventoryItemsPlace> places)
        {
            overflow = parameters.Count;
            changes = null;
            places = null;

            if (!isOwner)
            {
                Log.Error("Tried to add an item without being the owner");
                return false;
            }

            if (!CanAddItem(parameters, out overflow, out changes, out places))
            {
                if (!allowPartial)
                {
                    return false;
                }
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
        public bool TryPlaceItem(InventoryPlaceParams parameters, bool allowPartial, out int overflow, out NetInventoryItemsPlace place, out NetInventoryItemsChange change)
        {
            overflow = parameters.Count;
            place = default;
            change = default;

            if (!isOwner)
            {
                Log.Error("Tried to place an item without being the owner");
                return false;
            }

            if (!CanPlaceItem(parameters, out overflow, out place, out change) || overflow > 0)
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

            return allowPartial ? overflow < parameters.Count : true;
        }

        /// <summary>
        /// Tries to remove the given count of an item from the inventory
        /// </summary>
        public bool TryRemoveItem(InventoryChangeParams parameters, bool allowPartial, out int remaining, out List<NetInventoryItemsChange> changes)
        {
            remaining = parameters.Count;
            changes = null;

            if (!isOwner)
            {
                Log.Error("Tried to remove an item without being the owner");
                return false;
            }

            if (!CanRemoveItem(parameters, out remaining, out changes))
            {
                if (!allowPartial)
                {
                    return false;
                }
            }

            foreach (NetInventoryItemsChange change in changes)
            {
                ProcessNetInventoryItemsChange(change);
            }

            return true;
        }

        public bool CanAddItem(InventoryChangeParams addParams, out int overflow, out List<NetInventoryItemsChange> changes, out List<NetInventoryItemsPlace> places)
        {
            overflow = addParams.Count;
            changes = new();
            places = new();

            ItemData data = _itemManager.GetItemData(addParams.ItemId);

            if (data == null || addParams.Count <= 0)
            {
                Log.Error("Checked if an invalid item can be added");
                return false;
            }

            // Check matching instances
            if (data.MaxStack > 1)
            {
                foreach (NetInventoryItem netInventoryItem in _netInventoryItems.Values)
                {
                    if (netInventoryItem.ItemId != addParams.ItemId)
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

            List<Vector2Int> placedCells = new();
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

                    int placeCount = Mathf.Min(overflow, data.MaxStack);

                    InventoryPlaceParams placeParams = new InventoryPlaceParams()
                    {
                        Cell = kvp.Key,
                        RotationParams = new InventoryRotationParams() { AutoFit = true },
                        InstanceId = addParams.InstanceId,
                        ItemId = addParams.ItemId,
                        Count = placeCount
                    };

                    if (!CanPlaceItem(placeParams, out _, out NetInventoryItemsPlace place, out _))
                    {
                        continue;
                    }

                    // We can assume the full count is placed, since we are only checking empty slots
                    overflow -= placeCount;

                    if (place.IsValid)
                    {
                        places.Add(place);
                        AddPlacedCells(place.Parameters.Cell, place.Shape);
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
        public bool CanPlaceItem(InventoryPlaceParams parameters, out int overflow, out NetInventoryItemsPlace place, out NetInventoryItemsChange change)
        {
            overflow = parameters.Count;
            place = default;
            change = default;

            ItemData data = _itemManager.GetItemData(parameters.ItemId);

            if (data == null || parameters.Count <= 0 || parameters.Count > data.MaxStack)
            {
                Log.Error("Checked if an invalid item can be placed");
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
                return netInventoryItem.ItemId == parameters.ItemId && netInventoryItem.CanAddCount(parameters.Count, out overflow, out change);
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

            parameters.RotationParams = new InventoryRotationParams() { Rotations = rotations };

            parameters.Count = Mathf.Min(parameters.Count, data.MaxStack);

            overflow -= parameters.Count;
            place = new NetInventoryItemsPlace(parameters, placeShape);

            return true;
        }

        public bool CanRemoveItem(InventoryChangeParams parameters, out int remaining, out List<NetInventoryItemsChange> changes)
        {
            remaining = parameters.Count;
            changes = new();
            
            ItemData data = _itemManager.GetItemData(parameters.ItemId);

            if (data == null || parameters.Count <= 0)
            {
                Log.Error("Checked if an invalid item can be removed");
                return false;
            }

            foreach (NetInventoryItem netInventoryItem in _netInventoryItems.Values)
            {
                if (netInventoryItem.ItemId != parameters.ItemId)
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
            string instanceId = place.Parameters.InstanceId != null ? place.Parameters.InstanceId : _networkManager.LocalPurrnetPlayer.GetNextItemInstanceId();

            NetInventoryItem item = new NetInventoryItem(place.Parameters.Cell, place.Parameters.Pivot, place.Parameters.RotationParams.Rotations, instanceId, place.Parameters.ItemId, place.Parameters.Count);

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