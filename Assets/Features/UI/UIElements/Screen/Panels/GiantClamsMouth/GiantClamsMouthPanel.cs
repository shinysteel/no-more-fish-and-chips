using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class GiantClamsMouthPanel : Panel
    {
        [SerializeField] private InventoryWidget _playerInventoryWidget;
        [SerializeField] private InventoryWidget _clamInventoryWidget;

        public void Setup(GameplayContext context, Inventory clamInventory)
        {
            _playerInventoryWidget.Setup(context, context.LocalPlayer.Inventory);
            _clamInventoryWidget.Setup(context, clamInventory);
        }
    }
}