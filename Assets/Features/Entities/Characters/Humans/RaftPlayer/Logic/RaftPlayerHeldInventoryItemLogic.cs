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
        private CharacterModel _playerModel;

        private ItemModel _heldModel;

        public RaftPlayerHeldInventoryItemLogic(RaftPlayer player, CharacterModel playerModel)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _player = player;
            _playerModel = playerModel;

            HandleHotbarSelectedChanged(_player.Hotbar.SelectedIndex, _player.Hotbar.SelectedItem);

            _player.Hotbar.OnSelectedChanged += HandleHotbarSelectedChanged;
        }

        ~RaftPlayerHeldInventoryItemLogic()
        {
            if (_player != null)
            {
                _player.Hotbar.OnSelectedChanged -= HandleHotbarSelectedChanged;
            }
        }

        private void HandleHotbarSelectedChanged(int index, InventoryItem item)
        {
            if (_heldModel != null && _heldModel.ItemId != item?.ItemInstance.Data.ItemId)
            {
                _poolManager.ReturnItemModel(_heldModel);
                _heldModel = null;
            }
            
            if (_heldModel == null && item != null)
            {
                _heldModel = _poolManager.GetItemModel(item.ItemInstance.Data.ItemId, new SpawnParams() { Parent = _playerModel.ItemLocator });
            }
        }
    }
}