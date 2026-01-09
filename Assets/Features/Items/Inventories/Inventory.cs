using PurrNet;
using ShinyOwl.Common.Structures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishFlingers.Networking;

namespace FishFlingers.Items
{
    public class InventoryItemInstance
    {

    }

    public class InventorySlot
    {
        
    }

    public class Inventory : NetBehaviour, IEnumerable<KeyValuePair<Vector2Int, InventorySlot>>
    {
        private SyncDictionary<Vector2Int, InventorySlot> _cellSlotMap = new(ownerAuth: true);

        public void Initialise(BoolGrid grid)
        {
            for (int i = 0; i < grid.Columns; i++)
            {
                for (int j = 0; j < grid.Rows; j++)
                {
                    if (grid[i, j] == true)
                    {
                        _cellSlotMap.Add(new Vector2Int(i, j), new());
                    }
                }
            }
        }

        public IEnumerator<KeyValuePair<Vector2Int, InventorySlot>> GetEnumerator()
        {
            return _cellSlotMap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // add items
        // pick up an item
        // place an item
        // spend an item
    }
}