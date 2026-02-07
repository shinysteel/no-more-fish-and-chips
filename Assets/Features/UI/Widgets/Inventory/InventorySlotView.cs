using FishFlingers.Inventories;
using FishFlingers.Pools;
using TMPro;
using UnityEngine;

namespace FishFlingers.UI
{
    // Ambigious with FishFlingers.Inventories.InventorySlot, so we use the View suffix
    public class InventorySlotView : MonoBehaviour, IPoolable
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TMP_Text _cellText;

        private InventoryWidget _inventoryWidget;
        private Vector2Int _cell;

        public RectTransform RectTransform => _rectTransform;
        public InventoryWidget InventoryWidget => _inventoryWidget;
        public Vector2Int Cell => _cell;

        public void Setup(InventoryWidget inventoryWidget, Vector2Int cell)
        {
            _inventoryWidget = inventoryWidget;
            _cell = cell;

            _cellText.text = $"({cell.x}, {cell.y})";
        }

        public void SetTransform(Vector2 position, Vector2 size)
        {
            _rectTransform.anchoredPosition = position;
            _rectTransform.sizeDelta = size;
        }

        public void OnReturnedToPool() { }
        public void OnTakenFromPool() { }
    }
}