using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class GiantClamsMouthPanel : Panel
    {
        [SerializeField] private Image _timerFillImage;
        [SerializeField] private InventoryWidget _playerInventoryWidget;
        [SerializeField] private InventoryWidget _clamInventoryWidget;

        private GameplayContext _context;
        private GiantClam _clam;

        public void Setup(GameplayContext context, GiantClam clam, Inventory clamInventory)
        {
            _context = context;
            _clam = clam;

            _playerInventoryWidget.Setup(_context, _context.LocalPlayer.Inventory);
            _clamInventoryWidget.Setup(_context, clamInventory);
        }

        public override void Show(Action onComplete)
        {
            base.Show(onComplete);

            _context.LocalPlayer.SetNetOpenObjectNetworkId(_clam);
        }

        public override void Hide(Action onComplete)
        {
            _context.LocalPlayer.SetNetOpenObjectNetworkId(null);

            base.Hide(onComplete);
        }

        private void Update()
        {
            if (_context == null)
            {
                return;
            }

            TimerUpdate();
        }

        private void TimerUpdate()
        {
            _timerFillImage.fillAmount = 1f - _clam.EntityLifecycleModule.TimeAlive / _clam.DefinitionData.AwaitItemsSettings.Duration;
        }
    }
}