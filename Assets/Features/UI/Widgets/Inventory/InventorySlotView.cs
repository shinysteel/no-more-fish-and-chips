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

        private Vector2Int _cell;

        public RectTransform RectTransform => _rectTransform;

        public void Setup(Vector2Int cell, Vector3 position)
        {
            _rectTransform.anchoredPosition = position;
            _cell = cell;

            _cellText.text = $"({cell.x}, {cell.y})";
        }

        public void OnReturnedToPool() { }
        public void OnTakenFromPool() { }
    }
}