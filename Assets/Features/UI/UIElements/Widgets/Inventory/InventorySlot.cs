using UnityEngine;

namespace FishFlingers.UI
{
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;

        private Vector2Int _cell;

        public RectTransform RectTransform => _rectTransform;

        public void Setup(Vector2Int cell, Vector3 position)
        {
            _rectTransform.anchoredPosition = position;
            _cell = cell;
        }
    }
}