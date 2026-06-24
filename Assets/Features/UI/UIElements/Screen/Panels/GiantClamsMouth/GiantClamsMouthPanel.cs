using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.States;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class GiantClamsMouthPanel : Panel
    {
        [SerializeField] private Image _timerFillImage;
        [SerializeField] private InventoryWidget _playerInventoryWidget;
        [SerializeField] private InventoryWidget _clamInventoryWidget;

        private GiantClam _clam;

        public void Setup(GameplayContext context, GiantClam clam, Inventory clamInventory)
        {
            _clam = clam;

            _playerInventoryWidget.Setup(context, context.LocalPlayer.Inventory);
            _clamInventoryWidget.Setup(context, clamInventory);
        }

        private void Update()
        {
            _timerFillImage.fillAmount = 1f - _clam.EntityLifecycleModule.TimeAlive / _clam.DefinitionData.AwaitItemsSettings.Duration;
        }
    }
}