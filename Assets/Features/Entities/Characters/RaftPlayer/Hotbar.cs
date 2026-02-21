using FishFlingers.Inventories;
using FishFlingers.States;
using NUnit.Framework;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Hotbar
{
    private List<InventoryItem> _slots = new List<InventoryItem>(DefaultCapacity);

    public IReadOnlyList<InventoryItem> Slots => _slots;

    private const int DefaultCapacity = 3;

    private GameplayContext _context;

    public event Action<int, InventoryItem> OnSlotChanged;

    public Hotbar(GameplayContext context)
    {
        _context = context;
        _context.LocalPlayer.Inventory.OnInventoryItemChanged += HandleInventoryItemChanged;

        for (int i = 0; i < DefaultCapacity; i++)
        {
            // Can't use .SetSlot, since it does index guards
            _slots.Add(null);
        }
    }

    ~Hotbar()
    {
        if (_context?.LocalPlayer?.Inventory != null)
        {
            _context.LocalPlayer.Inventory.OnInventoryItemChanged -= HandleInventoryItemChanged;
        }
    }

    private void HandleInventoryItemChanged(string instanceId, InventoryItem oldInventoryItem, InventoryItem newInventoryItem)
    {
        // Find and update a potential slot linked to the item
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i]?.ItemInstance.InstanceId != instanceId)
            {
                continue;
            }

            SetSlot(i, newInventoryItem);
            return;
        }
    }

    public void SetSlot(int index, InventoryItem item)
    {
        // Guard against invalid requests
        if (index < 0 || index >= _slots.Count)
        {
            return;
        }

        // You can't equip the same item in more than one slot, so we check for duplicates when assigning a value that isn't null
        if (item != null)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (i == index)
                {
                    continue;
                }

                if (_slots[i]?.ItemInstance.InstanceId == item.ItemInstance.InstanceId)
                {
                    return;
                }
            }
        }

        _slots[index] = item;
        
        OnSlotChanged?.Invoke(index, item);
    }
}
