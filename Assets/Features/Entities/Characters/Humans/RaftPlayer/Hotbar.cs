using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using Newtonsoft.Json;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class HotbarSlot
    {
        private Hotbar _hotbar;
        private Inventory _inventory;
        private int _index;

        private string _instanceId;
        private InventoryItem _inventoryItem;

        public int Index => _index;
        public string InstanceId => _instanceId;
        public InventoryItem InventoryItem => _inventoryItem;

        public HotbarSlot(Hotbar hotbar, Inventory inventory, int index)
        {
            _hotbar = hotbar;
            _inventory = inventory;
            _index = index;
        }

        public void SetInstanceId(string instanceId)
        {
            _instanceId = instanceId;

            Refresh();
        }

        // Determines if the cached item is dirty. If so, the value is updated and broadcasted
        public void Refresh()
        {
            InventoryItem newInventoryItem = null;

            if (_instanceId != null)
            {
                _inventory.InventoryItems.TryGetValue(_instanceId, out newInventoryItem);
            }

            if (_inventoryItem == newInventoryItem)
            {
                return;
            }

            _inventoryItem = newInventoryItem;

            _hotbar.NotifySlotChanged(this);
        }
    }

    public class Hotbar : NetBehaviour
    {
        [SerializeField] private Inventory _inventory;

        private SyncArray<string> _netSlots = new SyncArray<string>(length: DefaultCapacity, ownerAuth: true);
        private SyncVar<int> _netSelectedIndex = new SyncVar<int>(ownerAuth: true);

        private List<HotbarSlot> _slots = new List<HotbarSlot>(DefaultCapacity);
        public IReadOnlyList<HotbarSlot> Slots => _slots;

        private const int DefaultCapacity = 5;

        private HotbarSlot _selectedSlot;
        public HotbarSlot SelectedSlot => _selectedSlot;

        // Invoked when a slot is changed
        public event Action<HotbarSlot> OnSlotChanged;

        // Invoked when a slot is selected
        public event Action<HotbarSlot> OnSelectedChanged;

        protected override void Awake()
        {
            base.Awake();

            for (int i = 0; i < DefaultCapacity; i++)
            {
                // Can't use .SetSlot, since it does index guards
                _slots.Add(new HotbarSlot(this, _inventory, i));
            }

            _selectedSlot = _slots[0];
        }

        protected override void OnSpawned()
        {
            if (isOwner)
            {
                _inventory.OnInventoryItemChanged += HandleInventoryItemChanged;
            }
            else
            {
                for (int i = 0; i < _netSlots.Count; i++)
                {
                    HandleNetSlotsChanged(SyncArrayChange<string>.Set(_netSlots[i], null, i));
                }

                HandleNetSelectedIndexChanged(_netSelectedIndex.value);
            }
            
            _netSlots.onChanged += HandleNetSlotsChanged;
            _netSelectedIndex.onChanged += HandleNetSelectedIndexChanged;

            base.OnSpawned();
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            if (isOwner)
            {
                _inventory.OnInventoryItemChanged -= HandleInventoryItemChanged;   
            }

            _netSlots.onChanged -= HandleNetSlotsChanged;
            _netSelectedIndex.onChanged -= HandleNetSelectedIndexChanged;
        }

        private void Update()
        {
            RefreshSlotsUpdate();
        }

        private void RefreshSlotsUpdate()
        {
            foreach (HotbarSlot slot in _slots)
            {
                slot.Refresh();
            }
        }

        private void HandleNetSlotsChanged(SyncArrayChange<string> change)
        {
            _slots[change.index].SetInstanceId(change.value);
        }

        public void NotifySlotChanged(HotbarSlot slot)
        {
            OnSlotChanged?.Invoke(slot);

            if (_selectedSlot.Index == slot.Index)
            {
                OnSelectedChanged?.Invoke(slot);
            }
        }
        
        private void HandleNetSelectedIndexChanged(int index)
        {
            _selectedSlot = _slots[index];

            OnSelectedChanged?.Invoke(_selectedSlot);
        }

        private void HandleInventoryItemChanged(string instanceId, InventoryItem oldInventoryItem, InventoryItem newInventoryItem)
        {
            bool assigned = false;

            // Find and update a potential slot linked to the item
            foreach (HotbarSlot slot in _slots)
            {
                if (slot.InventoryItem?.ItemInstance.InstanceId != instanceId)
                {
                    continue;
                }

                SetSlot(slot.Index, newInventoryItem?.ItemInstance.InstanceId);
                assigned = true;
                break;
            }

            // If it's not assigned and a slot is available, assign it
            if (oldInventoryItem == null && newInventoryItem != null && !assigned && TryGetNextUnassignedSlot(out int index))
            {
                SetSlot(index, newInventoryItem.ItemInstance.InstanceId);
            }
        }

        public void SetSlot(int index, string instanceId)
        {
            if (!isOwner)
            {
                return;
            }

            // Guard against invalid requests
            if (index < 0 || index >= _slots.Count)
            {
                return;
            }

            // You can't equip the same item in more than one slot, so we need to swap the existing assignment when assigning a value that isn't null
            if (instanceId != null)
            {
                foreach (HotbarSlot slot in _slots)
                {
                    if (slot.Index == index)
                    {
                        continue;
                    }

                    if (slot.InventoryItem?.ItemInstance.InstanceId == instanceId)
                    {
                        _netSlots[slot.Index] = null;
                        break;
                    }
                }
            }

            _netSlots[index] = instanceId;
        }

        public void ChangeSelectedIndex(int delta)
        {
            if (!isOwner)
            {
                return;
            }

            if (delta == 0)
            {
                return;
            }

            int index = Utils.Math.EuclideanModulo(_selectedSlot.Index + delta, _slots.Count);
            SetSelectedIndex(index);
        }

        public void SetSelectedIndex(int index)
        {
            if (!isOwner)
            {
                return;
            }

            if (_selectedSlot.Index == index)
            {
                return;
            }

            _netSelectedIndex.value = index;
        }

        public bool IsItemAssigned(InventoryItem item, out int index)
        {
            index = -1;

            if (item == null)
            {
                return false;
            }

            index = _slots.FindIndex(slot => slot.InventoryItem?.ItemInstance.InstanceId == item.ItemInstance.InstanceId);
            return index >= 0;
        }

        private bool TryGetNextUnassignedSlot(out int index)
        {
            index = -1;

            foreach (HotbarSlot slot in _slots)
            {
                if (slot.InventoryItem != null)
                {
                    continue;
                }

                index = slot.Index;
                break;
            }

            return index >= 0;
        }
    }
}