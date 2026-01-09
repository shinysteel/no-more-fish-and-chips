using FishFlingers.Items;
using FishFlingers.Networking;
using FishFlingers.States;
using System;
using UnityEngine;

namespace FishFlingers.UI
{
    public class FishingBagPanel : Panel
    {
        [SerializeField] private InventoryWidget _inventoryWidget;

        public void Setup(GameplayContext context)
        {
            _inventoryWidget.Setup(context.LocalPlayer.Inventory);
        }
    }
}