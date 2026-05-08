using FishFlingers.Inventories;
using FishFlingers.Items;
using FishFlingers.Networking;
using FishFlingers.States;
using FishFlingers.UI;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
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

        private RaftPlayer _player;
        private GameplayContext _context;

        private SyncVar<NetInventoryItem> _netGrabbedInventoryItem;

        private InventoryRaycaster _inventoryRaycaster;

        public RaftPlayerHotkeyLogic(RaftPlayer player, GameplayContext context, SyncVar<NetInventoryItem> netGrabbedInventoryItem)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _player = player;
            _context = context;

            _netGrabbedInventoryItem = netGrabbedInventoryItem;

            _inventoryRaycaster = new();
        }

        /// <summary>
        /// Resolves all hotkeys
        /// </summary>
        public void Tick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            if (_player.InputLogic.LeftClick)
            {
                LeftClick();
            }

            if (_player.InputLogic.RightClick)
            {
                RightClick();
            }

            if (_player.InputLogic.RotateItem)
            {
                RotateInventoryItem();
            }

            if (_player.InputLogic.DropItem)
            {
                DropItem();
            }

            if (_player.InputLogic.Interact)
            {
                Interact();
            }

            if (_player.InputLogic.TryGetScroll(out float scroll))
            {
                Scroll(_player.InputLogic.Scroll);
            }

            if (_player.InputLogic.TryGetNumber(out int number))
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
            else if (_player.GrabbedInventoryItemLogic.GrabbedInventoryItem == null)
            {
                LeftClickWithoutGrabbed();
            }
            else
            {
                LeftClickWithGrabbed();
            }
        }

        private void ExecuteItemLeftClick()
        {
            if (_player.Hotbar.SelectedSlot.InventoryItem == null)
            {
                return;
            }

            _player.Hotbar.SelectedSlot.InventoryItem.ItemInstance.Data.LeftClickAction?.Execute(_context);
        }

        /// <summary>
        /// Unassigns a hotbar slot or grabs an item
        /// </summary>
        private void LeftClickWithoutGrabbed()
        {
            _inventoryRaycaster.GetViews(out InventoryItemView itemView, out InventorySlotView inventorySlot, out HotbarWidgetSlot hotbarSlot, out _);

            if (_player.InputLogic.Shift)
            {
                MoveItem(itemView);
            }
            else if (hotbarSlot != null)
            {
                _player.Hotbar.SetSlot(hotbarSlot.Index, null);
            }
            else
            {
                GrabItem(itemView, inventorySlot);
            }
        }

        /// <summary>
        /// Moves an item from one inventory to another using auto fit
        /// </summary>
        private void MoveItem(InventoryItemView itemView)
        {
            if (itemView == null)
            {
                return;
            }

            if (_player.OpenNetBehaviourLogic.Behaviour is not IHasInventory hasInventory)
            {
                return;
            }

            Inventory fromInventory;
            Inventory toInventory;
            
            if (itemView.InventoryWidget.Inventory == _player.Inventory)
            {
                fromInventory = _player.Inventory;
                toInventory = hasInventory.Inventory;
            }
            else
            {
                fromInventory = hasInventory.Inventory;
                toInventory = _player.Inventory;
            }

            _ = _player.MoveInventoryItemRpc(fromInventory.owner.Value, fromInventory, toInventory, itemView.InventoryItem.ItemInstance.InstanceId);
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
            _ = _player.GrabbedInventoryItemLogic.GrabAsync(itemView, inventorySlot);
        }

        /// <summary>
        /// The grabbed item can be assigned, placed, or dropped
        /// </summary>
        private void LeftClickWithGrabbed()
        {
            _inventoryRaycaster.GetViews(out InventoryItemView itemView, out InventorySlotView inventorySlot, out HotbarWidgetSlot hotbarSlot, out Panel panel);

            if (hotbarSlot != null)
            {
                _player.GrabbedInventoryItemLogic.Assign(hotbarSlot);
            }
            else if (inventorySlot != null)
            {
                _ = _player.GrabbedInventoryItemLogic.PlaceAsync(inventorySlot);
            }
            else if (panel == null)
            {
                _ = _player.GrabbedInventoryItemLogic.DropAsync();
            }
        }

        private void RightClick()
        {
            if (!_player.CanAct)
            {
                return;
            }

            ExecuteItemRightClick();
        }

        private void ExecuteItemRightClick()
        {
            if (_player.Hotbar.SelectedSlot.InventoryItem == null)
            {
                return;
            }

            _player.Hotbar.SelectedSlot.InventoryItem.ItemInstance.Data.RightClickAction?.Execute(_context);
        }

        /// <summary>
        /// Rotates the grabbed item, or an item at the cursor
        /// </summary>
        private void RotateInventoryItem()
        {
            if (!_uiManager.IsLayerInUse(UILayer.Panels))
            {
                return;
            }

            if (_player.GrabbedInventoryItemLogic.GrabbedInventoryItem != null)
            {
                RotateGrabbedInventoryItem();
            }
            else
            {
                RotateInventoryItemAtCursor();
            }
        }

        private void RotateGrabbedInventoryItem()
        {
            _networkManager.ChangeSyncVar(_netGrabbedInventoryItem, () => _netGrabbedInventoryItem.value.ChangeRotations(1));
        }

        private void RotateInventoryItemAtCursor()
        {
            _inventoryRaycaster.GetViews(out InventoryItemView itemView, out InventorySlotView inventorySlot, out _, out _);

            if (itemView == null || inventorySlot == null)
            {
                return;
            }

            if (itemView.InventoryItem.ItemInstance.InstanceId != inventorySlot.InventoryItem?.ItemInstance.InstanceId)
            {
                return;
            }

            InventoryItem inventoryItem = itemView.InventoryItem.DeepClone();

            // The slot at the cursor becomes the pivot
            inventoryItem.SetPivot(InventoryItemUtils.RecalculatePivot(inventoryItem.Cell, inventorySlot.Cell, inventoryItem.Pivot, inventoryItem.Rotations));
            inventoryItem.ChangeRotations(1);

            InventoryPlaceParams parameters = InventoryPlaceParams.Create(inventorySlot.Cell, inventoryItem);
            itemView.InventoryWidget.Inventory.TryPlaceItem(parameters, false, out _, out _, out _);
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
            else if (_player.CanAct)
            {
                DropSelectedItem();
            }
        }

        /// <summary>
        /// Drops the item at the cursor, whether its an item view or hotbar slot
        /// </summary>
        private void DropItemAtCursor()
        {
            _inventoryRaycaster.GetViews(out InventoryItemView itemView, out _, out HotbarWidgetSlot hotbarSlot, out _);

            if (itemView?.InventoryItem != null)
            {
                Drop(itemView.InventoryItem.ItemInstance, itemView.InventoryWidget.Inventory);
            }
            else if (hotbarSlot?.InventoryItem != null)
            {
                Drop(hotbarSlot.InventoryItem.ItemInstance, _player.Inventory);
            }

            void Drop(ItemInstance instance, Inventory inventory)
            {
                _player.DropInventoryItemLogic.DropItem(instance);
                inventory.TryRemoveItem(instance.InstanceId);
            }
        }

        private void DropSelectedItem()
        {
            if (_player.Hotbar.SelectedSlot.InventoryItem == null)
            {
                return;
            }

            _player.DropInventoryItemLogic.DropItem(_player.Hotbar.SelectedSlot.InventoryItem.ItemInstance);
            _player.Inventory.TryRemoveItem(_player.Hotbar.SelectedSlot.InventoryItem.ItemInstance.InstanceId);
        }

        private void Interact()
        {
            if (!_player.CanAct)
            {
                return;
            }

            _player.InteractLogic.Interact();
        }

        private void Scroll(float scroll)
        {
            if (!_player.CanAct)
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

            _player.Hotbar.ChangeSelectedIndex(delta);
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
            else if (_player.CanAct)
            {
                _player.Hotbar.SetSelectedIndex(number - 1);
            }
        }

        private void AssignItem(int number)
        {
            if (_player.GrabbedInventoryItemLogic.GrabbedInventoryItem != null)
            {
                return;
            }

            _inventoryRaycaster.GetViews(out InventoryItemView itemView, out _, out _, out _);

            if (itemView == null)
            {
                return;
            }

            if (itemView.InventoryItem == null)
            {
                return;
            }

            if (itemView.InventoryWidget.Inventory != _player.Inventory)
            {
                return;
            }

            _player.Hotbar.SetSlot(number - 1, itemView.InventoryItem.ItemInstance.InstanceId);
        }
    }
}