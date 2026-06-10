using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using Newtonsoft.Json;
using ShinyOwl.Common.Utils;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace NoMoreFishAndChips.Inventories
{
    public class InventorySave
    {
        [JsonProperty] public List<InventoryItemSave> Items { get; private set; } = new();

        public InventorySave()
        { }

        public InventorySave(Inventory inventory)
        {
            SaveFrom(inventory);
        }

        public async Task LoadToAsync(Inventory inventory)
        {
            while (!inventory.IsReady)
            {
                await Task.Yield();
            }

            inventory.ClearNetInventoryItems();

            foreach (InventoryItemSave itemSave in Items)
            {
                inventory.TryPlaceItem(InventoryPlaceParams.Create(itemSave), false, out _, out _, out _);
            }
        }

        public void SaveFrom(Inventory inventory)
        {
            Items.Clear();

            foreach (InventoryItem item in inventory.InventoryItems.Values)
            {
                Items.Add(new InventoryItemSave(item));
            }
        }
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
}