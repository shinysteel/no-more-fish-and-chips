using FishFlingers.Inventories;
using UnityEngine;
using ShinyOwl.Common.Utils;
using System.Linq;
using FishFlingers.Pools;
using ShinyOwl.Common;

namespace FishFlingers.UI
{
    public class InventoryItemView : MonoBehaviour, IPoolable
    {
        // Composition instead of inheritance so that prefab variants play nicely
        [SerializeField] private ItemView _view;
       
        private InventoryWidget _inventoryWidget;

        public ItemView View => _view;
        public InventoryWidget InventoryWidget => _inventoryWidget;

        public void Setup(InventoryWidget inventoryWidget, InventoryItem inventoryItem)
        {
            _inventoryWidget = inventoryWidget;

            _view.SetSlotSize(_inventoryWidget.SlotSize);

            _view.Setup(inventoryItem);

            // No harm in calling _view.Refresh twice just so we can do one line here
            Refresh();
        }

        public void Refresh()
        {
            _view.Refresh();

            RefreshRect();
        }

        private void RefreshRect()
        {
            // Position
            InventorySlotView slotView = _inventoryWidget.InventorySlotViews[_view.InventoryItem.Cell];
            _view.RectTransform.anchoredPosition = slotView.View.RectTransform.anchoredPosition;
        }

        public void OnReturnedToPool() 
        {
            _view.ResetAlpha();
        }

        public void OnTakenFromPool() 
        { }
    }
}