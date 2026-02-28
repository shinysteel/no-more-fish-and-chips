using FishFlingers.Inventories;
using FishFlingers.Pools;
using FishFlingers.States;
using ShinyOwl.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    // Ambigious with FishFlingers.Inventories.InventorySlot, so we use the View suffix
    public class InventorySlotView : MonoBehaviour, ISlotView, IPoolable
    {
        [SerializeField] private SlotView _view;

        public RectTransform RectTransform => _view.RectTransform;
        public CellOutline CellOutline => _view.CellOutline;

        private InventoryWidget _inventoryWidget;
        private Vector2Int _cell;

        public InventoryWidget InventoryWidget => _inventoryWidget;
        public Vector2Int Cell => _cell;

        public InventoryItem InventoryItem => _view.InventoryItem;

        public void Setup(GameplayContext context, InventoryWidget inventoryWidget, Vector2Int cell)
        {
            _view.Setup(context);

            _inventoryWidget = inventoryWidget;

            _cell = cell;
        }

        public void SetTransform(Vector2 position, Vector2 size)
        {
            _view.SetTransform(position, size);
        }

        public void SetInventoryItem(InventoryItem item)
        {
            _view.SetInventoryItem(item);

            _view.RefreshColor();
        }

        public void OnReturnedToPool()
        {
            _view.OnDestroy();
        }

        public void OnTakenFromPool() { }
    }
}