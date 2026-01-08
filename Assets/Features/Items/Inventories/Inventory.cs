using System.Collections.Generic;
using UnityEngine;

namespace FishFlingers.Items
{
    public class InventorySlot
    {
        public InventoryItemInstance InventoryItemInstance { get; private set; }
    }

    public class Inventory
    {
        private Dictionary<Vector2Int, InventorySlot> _cellSlotMap;

        public void Initialise()
        {

        }

        // add items
        // pick up an item
        // place an item
        // spend an item
    }
}