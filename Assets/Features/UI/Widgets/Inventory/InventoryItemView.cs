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

            _view.Setup(inventoryItem, false);

            // No harm in calling _view.UpdateView twice just so we can do one line here
            UpdateView();
        }

        public void UpdateView()
        {
            _view.UpdateView();

            UpdateImage();
        }

        private void UpdateImage()
        {
            bool horizontal = _view.InventoryItem.Rotations % 2 == 0;
            int columns = horizontal ? _view.InventoryItem.Shape.Columns : _view.InventoryItem.Shape.Rows;
            int rows = horizontal ? _view.InventoryItem.Shape.Rows : _view.InventoryItem.Shape.Columns;

            // Position
            _view.RectTransform.pivot = new Vector2(1f / (columns * 2f), 1f / (rows * 2f));
            InventorySlotView pivotInventorySlotView = _inventoryWidget.InventorySlotViews[_view.InventoryItem.Pivot];
            _view.RectTransform.anchoredPosition = pivotInventorySlotView.RectTransform.anchoredPosition;
        }

        public void OnTakenFromPool() { }
        public void OnReturnedToPool() { }
    }
}