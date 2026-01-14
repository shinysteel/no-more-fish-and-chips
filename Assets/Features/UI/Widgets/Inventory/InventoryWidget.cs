using FishFlingers.Items;
using System.Collections.Generic;
using UnityEngine;
using FishFlingers.Inventories;
using FishFlingers.Pools;
using ShinyOwl.Common;

namespace FishFlingers.UI
{
    public class InventoryWidget : MonoBehaviour
    {
        [SerializeField] private Transform _inventorySlotViewsContainer;
        [SerializeField] private Transform _inventoryItemViewsContainer;

        [SerializeField] private float _slotSize = 60f;
        [SerializeField] private float _slotPadding = 5f;

        public float SlotSize => _slotSize;

        private PoolManager _poolManager;

        private Inventory _inventory;

        private Dictionary<Vector2Int, InventorySlotView> _inventorySlotViews;
        private Dictionary<string, InventoryItemView> _inventoryItemViews;

        public IReadOnlyDictionary<Vector2Int, InventorySlotView> InventorySlotViews => _inventorySlotViews;

        public void Setup(Inventory inventory)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _inventory = inventory;

            _inventorySlotViews = CreateInventorySlotViews();

            _inventoryItemViews = new();

            foreach (KeyValuePair<string, InventoryItem> kvp in _inventory.InventoryItems)
            {
                HandleInventoryItemChanged(kvp.Key, kvp.Value);
            }

            _inventory.OnInventoryItemChanged += HandleInventoryItemChanged;
        }

        private void OnDestroy()
        {
            if (_poolManager != null)
            {
                foreach (InventorySlotView view in _inventorySlotViews.Values)
                {
                    _poolManager.Return(view);
                }

                foreach (InventoryItemView view in _inventoryItemViews.Values)
                {
                    _poolManager.Return(view);
                }
            }

            if (_inventory != null)
            {
                _inventory.OnInventoryItemChanged -= HandleInventoryItemChanged;
            }
        }

        private Dictionary<Vector2Int, InventorySlotView> CreateInventorySlotViews()
        {
            Dictionary<Vector2Int, InventorySlotView> views = new();

            foreach (KeyValuePair<Vector2Int, NetInventorySlot> kvp in _inventory)
            {
                InventorySlotView view = _poolManager.Get<InventorySlotView>(new SpawnParams() { Parent = _inventorySlotViewsContainer });

                Vector2Int cell = kvp.Key;

                // Since cells are only positive, we need to use a pivot to center them in the widget
                float pivotX = (_inventory.Columns - 1) / 2f;
                float pivotY = (_inventory.Rows - 1) / 2f;

                Vector3 position = new Vector3(
                    (cell.x - pivotX) * view.RectTransform.sizeDelta.x + cell.x * _slotPadding, 
                    (cell.y - pivotY) * view.RectTransform.sizeDelta.y + kvp.Key.y * _slotPadding);

                view.Setup(new Vector2Int(kvp.Key.x, kvp.Key.y), position);

                views.Add(cell, view);
            }

            return views;
        }

        private void HandleInventoryItemChanged(string instanceId, InventoryItem inventoryItem)
        {
            if (inventoryItem != null)
            {
                SetInventoryItemView(instanceId, inventoryItem);
            }
            else
            {
                RemoveInventoryItemView(instanceId);
            }
        }

        private void SetInventoryItemView(string key, InventoryItem inventoryItem)
        {
            if (!_inventoryItemViews.ContainsKey(key))
            {
                _inventoryItemViews[key] = _poolManager.Get<InventoryItemView>(new SpawnParams() { Parent = _inventoryItemViewsContainer });   
            }

            InventoryItemView view = _inventoryItemViews[key];
            view.Setup(this, inventoryItem);
        }

        private void RemoveInventoryItemView(string key)
        {
            if (!_inventoryItemViews.ContainsKey(key))
            {
                Debugger.LogError(this, "Tried to remove a view that does not exist");
                return;
            }

            _poolManager.Return(_inventoryItemViews[key]);
            _inventoryItemViews[key] = null;
        }
    }
}