using FishFlingers.Inventories;
using FishFlingers.Pools;
using ShinyOwl.Common;
using TMPro;
using UnityEngine;

namespace FishFlingers.UI
{
    // Ambigious with FishFlingers.Inventories.InventorySlot, so we use the View suffix
    public class InventorySlotView : MonoBehaviour, IPoolable
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private CellOutline _cellOutline;

        private InventoryWidget _inventoryWidget;
        private Vector2Int _cell;

        private InventoryItem _inventoryItem;

        public RectTransform RectTransform => _rectTransform;
        public InventoryWidget InventoryWidget => _inventoryWidget;
        public Vector2Int Cell => _cell;

        public void Setup(InventoryWidget inventoryWidget, Vector2Int cell)
        {
            _inventoryWidget = inventoryWidget;
            _cell = cell;
        }

        public void SetTransform(Vector2 position, Vector2 size)
        {
            _rectTransform.anchoredPosition = position;
            _rectTransform.sizeDelta = size;
        }

        public void SetInventoryItem(InventoryItem item)
        {
            _inventoryItem = item;
        }

        public void RefreshOutline()
        {
            // Item refers to both having an item in this cell, and it also existing in a given direction (1 unit away)
            bool item = _inventoryItem != null;
            bool itemTop = _inventoryWidget.InventorySlotViews.TryGetValue(Cell + new Vector2Int(0, 1), out InventorySlotView topView) && topView._inventoryItem == _inventoryItem;
            bool itemLeft = _inventoryWidget.InventorySlotViews.TryGetValue(Cell + new Vector2Int(-1, 0), out InventorySlotView leftView) && leftView._inventoryItem == _inventoryItem;
            bool itemBottom = _inventoryWidget.InventorySlotViews.TryGetValue(Cell + new Vector2Int(0, -1), out InventorySlotView bottomView) && bottomView._inventoryItem == _inventoryItem;
            bool itemRight = _inventoryWidget.InventorySlotViews.TryGetValue(Cell + new Vector2Int(1, 0), out InventorySlotView rightView) && rightView._inventoryItem == _inventoryItem;

            _cellOutline.SetEnabled(!item || !itemTop, !item || !itemLeft, !item || !itemBottom, !item || !itemRight);

            _cellOutline.SetColor(item ? Color.white : Color.gray);
        }

        public void OnReturnedToPool() 
        {
            _inventoryItem = null;
        }

        public void OnTakenFromPool() { }
    }
}