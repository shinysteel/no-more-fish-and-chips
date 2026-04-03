using FishFlingers.Cameras;
using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.Items;
using FishFlingers.UI;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace FishFlingers.Entities
{
    public class RaftPlayerGrabbedInventoryItemLogic
    {
        private RaftPlayer _player;

        private SyncVar<NetInventoryItem> _netGrabbedInventoryItem;

        private InventoryItem _grabbedInventoryItem;
        private InventoryItemView _grabbedItemView;

        public InventoryItem GrabbedInventoryItem => _grabbedInventoryItem;

        public event Action<InventoryItem> OnGrabbedInventoryItemChanged;

        public RaftPlayerGrabbedInventoryItemLogic(RaftPlayer player, SyncVar<NetInventoryItem> netGrabbedInventoryItem)
        {
            _player = player;

            _netGrabbedInventoryItem = netGrabbedInventoryItem;
            _netGrabbedInventoryItem.onChanged += HandleNetGrabbedInventoryItemChanged;
        }

        ~RaftPlayerGrabbedInventoryItemLogic()
        {
            if (_netGrabbedInventoryItem != null)
            {
                _netGrabbedInventoryItem.onChanged -= HandleNetGrabbedInventoryItemChanged;
            }
        }

        /// <summary>
        /// Mark an item as 'grabbed', and visualise it on the cursor
        /// </summary>
        public async Task GrabAsync(InventoryItemView itemView, InventorySlotView slotView)
        {
            NetInventoryItem grabbedNetInventoryItem = await _player.GrabRpc(itemView.InventoryWidget.Inventory.owner.Value, itemView.InventoryWidget.Inventory, itemView.InventoryItem.ItemInstance.InstanceId, slotView.Cell);
            if (grabbedNetInventoryItem == null)
            {
                return;
            }

            if (_grabbedItemView != null)
            {
                return;
            }

            _grabbedItemView = itemView;

            _netGrabbedInventoryItem.value = grabbedNetInventoryItem;

            // Listen for changes while we hold it
            itemView.InventoryWidget.Inventory.OnInventoryItemChanged += HandleInventoryItemChanged;
        }

        /// <summary>
        /// Retrieve relevant views to target under the cursor
        /// </summary>
        public void Assign(HotbarWidgetSlot slot)
        {
            _player.Hotbar.SetSlot(slot.Index, _grabbedInventoryItem);

            _ = ReleaseAsync();
        }

        /// <summary>
        /// Place the grabbed item at an inventory slot
        /// </summary>
        public async Task PlaceAsync(InventorySlotView slotView)
        {
            InventoryPlaceParams placeParams = new InventoryPlaceParams()
            {
                Cell = slotView.Cell,
                Pivot = _netGrabbedInventoryItem.value.Pivot,
                RotationParams = new InventoryRotationParams() { Rotations = _netGrabbedInventoryItem.value.Rotations },
                InstanceId = _grabbedInventoryItem.ItemInstance.InstanceId,
                ItemId = _grabbedInventoryItem.ItemInstance.Data.ItemId,
                Count = _grabbedInventoryItem.ItemInstance.Count
            };

            int? overflow = await _player.PlaceRpc(slotView.InventoryWidget.Inventory.owner.Value, slotView.InventoryWidget.Inventory, placeParams);

            if (overflow == null)
            {
                return;
            }

            await _player.SetRpc(_grabbedItemView.InventoryWidget.Inventory.owner.Value, _grabbedItemView.InventoryWidget.Inventory,
                _grabbedInventoryItem.ItemInstance.InstanceId, overflow.Value, _grabbedItemView.InventoryWidget.Inventory != slotView.InventoryWidget.Inventory);

            if (overflow == 0)
            {
                await ReleaseAsync();
            }
        }

        /// <summary>
        /// Drop the grabbed item out of the inventory
        /// </summary>
        public async Task DropAsync()
        {
            await _player.DropRpc(_grabbedItemView.InventoryWidget.Inventory.owner.Value, _grabbedItemView.InventoryWidget.Inventory, _grabbedInventoryItem.ItemInstance.InstanceId);

            _player.DropInventoryItemLogic.SpawnDroppedItem(_grabbedInventoryItem.ItemInstance, true);

            await ReleaseAsync();
        }

        /// <summary>
        /// Call this after a grab action is resolved to do necessary cleanup
        /// </summary>
        private async Task ReleaseAsync()
        {
            await _player.ReleaseRpc(_grabbedItemView.InventoryWidget.Inventory.owner.Value, _grabbedItemView.InventoryWidget.Inventory, _grabbedItemView.InventoryItem.ItemInstance.InstanceId);

            _grabbedItemView.InventoryWidget.Inventory.OnInventoryItemChanged -= HandleInventoryItemChanged;

            _netGrabbedInventoryItem.value = null;

            _grabbedItemView = null;
        }

        /// <summary>
        /// Broadcasts changes to the net grabbed item in a nicer format
        /// </summary>
        private void HandleNetGrabbedInventoryItemChanged(NetInventoryItem netInventoryItem)
        {
            _grabbedInventoryItem = netInventoryItem != null ? InventoryItem.Create(netInventoryItem) : null;

            OnGrabbedInventoryItemChanged?.Invoke(_grabbedInventoryItem);
        }

        /// <summary>
        /// If the source of the item we are holding has changes, we need to match them
        /// </summary>
        private void HandleInventoryItemChanged(string instanceId, InventoryItem oldInventoryItem, InventoryItem newInventoryItem)
        {
            if (_netGrabbedInventoryItem.value == null)
            {
                return;
            }

            if (_netGrabbedInventoryItem.value.ItemInstance.InstanceId != instanceId)
            {
                return;
            }

            // This callback can happen before we call SetNetGrabbedInventoryItem(null) ourselves, so it's safe to ignore in this scenario
            if (newInventoryItem == null)
            {
                return;
            }

            // Sync up with any changes that aren't to the pivot or rotations
            NetInventoryItem netInventoryItem = new NetInventoryItem(newInventoryItem.Cell, _netGrabbedInventoryItem.value.Pivot, _netGrabbedInventoryItem.value.Rotations, NetItemInstance.Create(newInventoryItem.ItemInstance));

            _netGrabbedInventoryItem.value = netInventoryItem;
        }
    }
}