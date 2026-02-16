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

        public RectTransform RectTransform => _rectTransform;
        public CellOutline CellOutline => _cellOutline;

        private InventoryWidget _inventoryWidget;
        private Vector2Int _cell;

        public InventoryWidget InventoryWidget => _inventoryWidget;
        public Vector2Int Cell => _cell;

        private InventoryItem _inventoryItem;

        public InventoryItem InventoryItem => _inventoryItem;

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
        
        public void OnReturnedToPool() 
        {
            _inventoryItem = null;
        }

        public void OnTakenFromPool() { }
    }
}