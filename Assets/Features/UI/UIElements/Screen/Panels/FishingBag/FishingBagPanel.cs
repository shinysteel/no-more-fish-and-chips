using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class FishingBagPanel : Panel
    {
        [SerializeField] private InventoryWidget _inventoryWidget;
        [SerializeField] private EquipmentWidget _equipmentWidget;
        [SerializeField] private HotbarWidget _hotbarWidget;

        public void Setup(GameplayContext context)
        {
            _inventoryWidget.Setup(context, context.LocalPlayer.Inventory);
            _ = _equipmentWidget.SetupAsync(context);
            _ = _hotbarWidget.SetupAsync(context);
        }
    }
}