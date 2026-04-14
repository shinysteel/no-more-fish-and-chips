using FishFlingers.Inventories;
using FishFlingers.States;
using ShinyOwl.Common;
using System;
using UnityEngine;
using PurrNet;

namespace FishFlingers.UI
{
    public class ClamChestPanel : Panel
    {
        [SerializeField] private InventoryWidget _playerInventoryWidget;
        [SerializeField] private InventoryWidget _chestInventoryWidget;

        private GameplayContext _context;

        public void Setup(GameplayContext context, Inventory chestInventory)
        {
            _context = context;
            _playerInventoryWidget.Setup(_context, _context.LocalPlayer.Inventory);
            _chestInventoryWidget.Setup(_context, chestInventory);
        }

        public override void Hide(Action onComplete)
        {
            _context.LocalPlayer.SetNetOpenObjectNetworkId(null);

            base.Hide(onComplete);
        }
    }
}