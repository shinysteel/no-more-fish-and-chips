using FishFlingers.Inventories;
using FishFlingers.Networking;
using FishFlingers.States;
using FishFlingers.UI;
using PurrNet;
using ShinyOwl.Common.Utils;
using UnityEngine;
using NetworkManager = FishFlingers.Networking.NetworkManager;

namespace FishFlingers.Entities
{
    /// <summary>
    /// Groups all hotkey outputs together so they can be resolved deterministically
    /// </summary>
    public class RaftPlayerHotkeyLogic
    {
        private UIManager _uiManager;
        private NetworkManager _networkManager;

        private GameplayContext _context;

        private SyncVar<NetInventoryItem> _netGrabbedInventoryItem;

        private InventoryRaycaster _inventoryRaycaster;

        public RaftPlayerHotkeyLogic(GameplayContext context, SyncVar<NetInventoryItem> netGrabbedInventoryItem)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _context = context;

            _netGrabbedInventoryItem = netGrabbedInventoryItem;

            _inventoryRaycaster = new();
        }

        /// <summary>
        /// Resolves all hotkeys
        /// </summary>
        public void Tick()
        {
            if (_context.LocalPlayer.InputLogic.LeftClick)
            {
                LeftClick();
            }

            if (_context.LocalPlayer.InputLogic.RightClick)
            {
                RightClick();
            }

            if (_context.LocalPlayer.InputLogic.RotateItem)
            {
                RotateItem();
            }

            if (_context.LocalPlayer.InputLogic.DropItem)
            {
                DropItem();
            }

            if (_context.LocalPlayer.InputLogic.Interact)
            {
                Interact();
            }

            if (_context.LocalPlayer.InputLogic.TryGetScroll(out float scroll))
            {
                Scroll(_context.LocalPlayer.InputLogic.Scroll);
            }

            if (_context.LocalPlayer.InputLogic.TryGetNumber(out int number))
            {
                Number(number);
            }
        }

        /// <summary>
        /// Can use an item's left click action, or interact with an inventory widget
        /// </summary>
        private void LeftClick()
        {
            if (!_uiManager.IsLayerInUse(UILayer.Panels))
            {
                ExecuteItemLeftClick();
                
            }
            else if (_context.LocalPlayer.GrabbedItemLogic.GrabbedInventoryItem == null)
            {
                LeftClickWithoutGrabbed();
            }
            else
            {
                LeftClickWithGrabbed();
            }
        }

        /// <summary>
        /// Unassigns a hotbar slot or grabs an item
        /// </summary>
        private void LeftClickWithoutGrabbed()
        {
            _inventoryRaycaster.GetViews(out InventoryItemView itemView, out InventorySlotView inventorySlot, out HotbarWidgetSlot hotbarSlot, out _);

            if (hotbarSlot != null)
            {
                _context.LocalPlayer.Hotbar.SetSlot(hotbarSlot.Index, null);
            }
            else
            {
                GrabItem(itemView, inventorySlot);
            }
        }

        private void GrabItem(InventoryItemView itemView, InventorySlotView inventorySlot)
        {
            if (itemView == null || inventorySlot == null)
            {
                return;
            }

            if (itemView.InventoryItem.ItemInstance.InstanceId != inventorySlot.InventoryItem?.ItemInstance.InstanceId)
            {
                return;
            }

            // Since the item is linked to the slot, grab it
            _context.LocalPlayer.GrabbedItemLogic.Grab(itemView, inventorySlot);
        }

        /// <summary>
        /// The grabbed item can be assigned, placed, or dropped
        /// </summary>
        private void LeftClickWithGrabbed()
        {
            _inventoryRaycaster.GetViews(out InventoryItemView itemView, out InventorySlotView inventorySlot, out HotbarWidgetSlot hotbarSlot, out Panel panel);

            if (hotbarSlot != null)
            {
                _context.LocalPlayer.GrabbedItemLogic.Assign(hotbarSlot);
            }
            else if (inventorySlot != null)
            {
                _context.LocalPlayer.GrabbedItemLogic.Place(inventorySlot);
            }
            else if (panel == null)
            {
                _context.LocalPlayer.GrabbedItemLogic.Drop();
            }
        }

        private void ExecuteItemLeftClick()
        {
            if (_context.LocalPlayer.Hotbar.SelectedItem == null)
            {
                return;
            }

            _context.LocalPlayer.Hotbar.SelectedItem.ItemInstance.Data.LeftClickAction?.Execute(_context);
        }

        private void RightClick()
        {
            if (!_context.LocalPlayer.CanAct)
            {
                return;
            }

            ExecuteItemRightClick();
        }

        private void ExecuteItemRightClick()
        {
            if (_context.LocalPlayer.Hotbar.SelectedItem == null)
            {
                return;
            }

            _context.LocalPlayer.Hotbar.SelectedItem.ItemInstance.Data.RightClickAction?.Execute(_context);
        }

        /// <summary>
        /// Rotates the grabbed item, or an item at the cursor
        /// </summary>
        private void RotateItem()
        {
            if (!_uiManager.IsLayerInUse(UILayer.Panels))
            {
                return;
            }

            if (_context.LocalPlayer.GrabbedItemLogic.GrabbedInventoryItem != null)
            {
                RotateGrabbedItem();
            }
            else
            {
                RotateItemAtCursor();
            }
        }

        private void RotateGrabbedItem()
        {
            _networkManager.ChangeSyncVar(_netGrabbedInventoryItem, () => _netGrabbedInventoryItem.value.ChangeRotations(1));
        }

        private void RotateItemAtCursor()
        {
            // Not implemented yet, but InventoryItem was made IDeepCloneable so that we could implement
            // this behaviour. It's just a bit tricky with modifying the pivot
        }

        /// <summary>
        /// Drops the item at the cursor, or the selected item
        /// </summary>
        private void DropItem()
        {
            if (_uiManager.IsLayerInUse(UILayer.Panels))
            {
                DropItemAtCursor();
            }
            else if (_context.LocalPlayer.CanAct)
            {
                DropSelectedItem();
            }
        }

        private void DropItemAtCursor()
        {
            _inventoryRaycaster.GetViews(out InventoryItemView itemView, out _, out _, out _);

            if (itemView?.InventoryItem == null)
            {
                return;
            }

            _context.LocalPlayer.DropItemLogic.SpawnDroppedItem(itemView.InventoryItem.ItemInstance, false);
            _context.LocalPlayer.Inventory.RemoveItem(itemView.InventoryItem.ItemInstance.InstanceId);
        }

        private void DropSelectedItem()
        {
            if (_context.LocalPlayer.Hotbar.SelectedItem == null)
            {
                return;
            }

            _context.LocalPlayer.DropItemLogic.SpawnDroppedItem(_context.LocalPlayer.Hotbar.SelectedItem.ItemInstance, true);
            _context.LocalPlayer.Inventory.RemoveItem(_context.LocalPlayer.Hotbar.SelectedItem.ItemInstance.InstanceId);
        }

        private void Interact()
        {
            if (!_context.LocalPlayer.CanAct)
            {
                return;
            }

            _context.LocalPlayer.InteractLogic.Interact();
        }

        private void Scroll(float scroll)
        {
            if (!_context.LocalPlayer.CanAct)
            {
                return;
            }

            ScrollHotbar(-scroll);
        }

        private void ScrollHotbar(float scroll)
        {
            int delta = (int)Mathf.Sign(scroll);

            if (delta == 0)
            {
                return;
            }

            _context.LocalPlayer.Hotbar.ChangeSelectedIndex(delta);
        }

        /// <summary>
        /// Assigns an item, or changes the hotbar's selected index
        /// </summary>
        private void Number(int number)
        {
            if (_uiManager.IsLayerInUse(UILayer.Panels))
            {
                AssignItem(number);
            }
            else if (_context.LocalPlayer.CanAct)
            {
                _context.LocalPlayer.Hotbar.SetSelectedIndex(number - 1);
            }
        }

        private void AssignItem(int number)
        {
            if (_context.LocalPlayer.GrabbedItemLogic.GrabbedInventoryItem != null)
            {
                return;
            }

            _inventoryRaycaster.GetViews(out InventoryItemView itemView, out _, out _, out _);

            if (itemView?.InventoryItem == null)
            {
                return;
            }

            _context.LocalPlayer.Hotbar.SetSlot(number - 1, itemView.InventoryItem);
        }
    }
}