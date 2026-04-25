using FishFlingers.Inventories;
using FishFlingers.Items;
using FishFlingers.Pools;
using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class RaftPlayerHeldInventoryItemLogic
    {
        private PoolManager _poolManager;

        private RaftPlayer _player;

        private ItemModel _heldModel;

        public ItemModel HeldModel => _heldModel;

        public RaftPlayerHeldInventoryItemLogic(RaftPlayer player)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _player = player;

            HandleHotbarSelectedChanged(_player.Hotbar.SelectedSlot);

            _player.Hotbar.OnSelectedChanged += HandleHotbarSelectedChanged;
        }

        ~RaftPlayerHeldInventoryItemLogic()
        {
            if (_player != null)
            {
                _player.Hotbar.OnSelectedChanged -= HandleHotbarSelectedChanged;
            }
        }

        private void HandleHotbarSelectedChanged(HotbarSlot slot)
        {
            if (_heldModel != null && _heldModel.ItemId != slot.InventoryItem?.ItemInstance.Data.ItemId)
            {
                _poolManager.ReturnItemModel(_heldModel);
                _heldModel = null;
            }
            
            if (_heldModel == null && slot.InventoryItem != null)
            {
                // Items need to be corrected by 90 degrees on the y-axis when held
                _heldModel = _poolManager.GetItemModel(slot.InventoryItem.ItemInstance.Data.ItemId, new SpawnParams() 
                { 
                    Rotation = Quaternion.AngleAxis(90f, Vector3.up),
                    Parent = _player.CharacterModel.ItemLocator
                });
            }
        }
    }
}