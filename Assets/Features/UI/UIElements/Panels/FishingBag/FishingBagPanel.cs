using FishFlingers.Items;
using System;
using UnityEngine;

namespace FishFlingers.UI
{
    public class FishingBagPanel : Panel
    {
        [SerializeField] private InventoryWidget _inventoryWidget;

        public override void Load()
        {
            base.Load();

            _inventoryWidget.Load();
        }

        public override void Show(Action onComplete)
        {
            base.Show(onComplete);

            _inventoryWidget.Show(null);
        }

        public override void Hide(Action onComplete)
        {
            base.Hide(onComplete);

            _inventoryWidget.Hide(null);
        }

        public override void Unload()
        {
            _inventoryWidget.Unload();

            base.Unload();
        }
    }
}