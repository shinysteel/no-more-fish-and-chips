using NoMoreFishAndChips.Inventories;
using UnityEngine;
using ShinyOwl.Common.Utils;
using System.Linq;
using NoMoreFishAndChips.Pools;
using ShinyOwl.Common;
using NoMoreFishAndChips.States;

namespace NoMoreFishAndChips.UI
{
    public class InventoryItemView : ItemView, ITypedPoolable
    {
        private InventoryWidget _inventoryWidget;
        public InventoryWidget InventoryWidget => _inventoryWidget;

        // When an item is 'grabbed', it's alpha is modified until the grab is resolved
        private const float UnavailableAlpha = 0.5f;
        private const float DefaultAlpha = 1f;

        public void SetInventoryWidget(InventoryWidget widget)
        {
            _inventoryWidget = widget;

            SetSlotSize(_inventoryWidget.SlotSize);
        }

        public override void Refresh()
        {
            base.Refresh();

            RefreshRect();
            RefreshAlpha();
        }

        private void RefreshRect()
        {
            // Position
            InventorySlotView slotView = _inventoryWidget.InventorySlotViews[_inventoryItem.Cell];
            _rectTransform.anchoredPosition = slotView.RectTransform.anchoredPosition;
        }

        private void RefreshAlpha()
        {
            // Alpha
            float alpha = _inventoryItem.IsAvailable ? DefaultAlpha : UnavailableAlpha;
            SetAlpha(alpha);
        }

        public void OnReturnedToPool() 
        {
            ResetAlpha();
        }

        public void OnTakenFromPool() 
        { }
    }
}