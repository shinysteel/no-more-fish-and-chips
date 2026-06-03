using FishFlingers.Inventories;
using UnityEngine;
using ShinyOwl.Common.Utils;
using System.Linq;
using FishFlingers.Pools;
using ShinyOwl.Common;
using FishFlingers.States;

namespace FishFlingers.UI
{
    public class InventoryItemView : MonoBehaviour, ITypedPoolable
    {
        // Composition instead of inheritance so that prefab variants play nicely
        [SerializeField] private ItemView _view;
       
        private InventoryWidget _inventoryWidget;
        public InventoryWidget InventoryWidget => _inventoryWidget;

        public InventoryItem InventoryItem => _view.InventoryItem;

        // When an item is 'grabbed', it's alpha is modified until the grab is resolved
        private const float UnavailableAlpha = 0.5f;
        private const float DefaultAlpha = 1f;

        public void Setup(InventoryWidget inventoryWidget, GameplayContext context, InventoryItem inventoryItem)
        {
            _inventoryWidget = inventoryWidget;

            _view.SetSlotSize(_inventoryWidget.SlotSize);

            _view.Setup(context, inventoryItem);

            // No harm in calling _view.Refresh twice just so we can do one line here
            Refresh();
        }

        public void Refresh()
        {
            _view.Refresh();

            RefreshRect();
            RefreshAlpha();
        }

        private void RefreshRect()
        {
            // Position
            InventorySlotView slotView = _inventoryWidget.InventorySlotViews[_view.InventoryItem.Cell];
            _view.RectTransform.anchoredPosition = slotView.RectTransform.anchoredPosition;
        }

        private void RefreshAlpha()
        {
            // Alpha
            float alpha = _view.InventoryItem.IsAvailable ? DefaultAlpha : UnavailableAlpha;
            _view.SetAlpha(alpha);
        }
        
        public void SetSlotSize(Vector2 size)
        {
            _view.SetSlotSize(size);
        }

        public void OnReturnedToPool() 
        {
            _view.ResetAlpha();
        }

        public void OnTakenFromPool() 
        { }
    }
}