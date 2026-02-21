using FishFlingers.Items;
using FishFlingers.Networking;
using FishFlingers.States;
using ShinyOwl.Common;
using System;
using UnityEngine;

namespace FishFlingers.UI
{
    public class FishingBagPanel : Panel
    {
        [SerializeField] private InventoryWidget _inventoryWidget;
        [SerializeField] private HotbarWidget _hotbarWidget;

        public void Setup(GameplayContext context)
        {
            _inventoryWidget.Setup(context, context.LocalPlayer.Inventory);
            _ = _hotbarWidget.SetupAsync(context);
        }
    }
}