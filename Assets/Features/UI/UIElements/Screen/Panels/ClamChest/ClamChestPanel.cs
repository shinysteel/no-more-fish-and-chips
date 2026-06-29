using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using System;
using UnityEngine;
using PurrNet;
using NoMoreFishAndChips.Entities;

namespace NoMoreFishAndChips.UI
{
    public class ClamChestPanel : Panel
    {
        [SerializeField] private InventoryWidget _playerInventoryWidget;
        [SerializeField] private InventoryWidget _chestInventoryWidget;

        private GameplayContext _context;

        private ClamChest _chest;

        public void Setup(GameplayContext context, ClamChest chest, Inventory chestInventory)
        {
            _context = context;
            _chest = chest;

            _playerInventoryWidget.Setup(_context, _context.LocalPlayer.Inventory);
            _chestInventoryWidget.Setup(_context, chestInventory);
        }

        public override void Show(Action onComplete)
        {
            base.Show(onComplete);

            _context.LocalPlayer.SetNetOpenObjectNetworkId(_chest);
        }

        public override void Hide(Action onComplete)
        {
            _context.LocalPlayer.SetNetOpenObjectNetworkId(null);

            _context = null;

            base.Hide(onComplete);
        }
    }
}