using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Pools;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerEquippedInventoryItemsLogic : RaftPlayerLogic
    {
        public RaftPlayerEquippedInventoryItemsLogic(RaftPlayer player) : base(player)
        {
            HandleHotbarSelectedChanged(_player.Hotbar.SelectedSlot);

            _player.Hotbar.OnSelectedChanged += HandleHotbarSelectedChanged;
        }

        ~RaftPlayerEquippedInventoryItemsLogic()
        {
            if (_player != null)
            {
                _player.Hotbar.OnSelectedChanged -= HandleHotbarSelectedChanged;
            }
        }

        private void HandleHotbarSelectedChanged(HotbarSlot slot)
        {
            _player.HumanModel.HoldItem(slot.InventoryItem?.ItemInstance.Data.ItemId ?? ItemId.None);
        }
    }
}