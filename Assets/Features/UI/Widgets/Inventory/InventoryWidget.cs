using FishFlingers.Items;
using System.Collections.Generic;
using UnityEngine;
using FishFlingers.Inventories;
using FishFlingers.Pools;
using ShinyOwl.Common;
using PurrNet.Prediction;
using FishFlingers.States;

namespace FishFlingers.UI
{
    public class InventoryWidget : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Transform _inventorySlotViewsContainer;
        [SerializeField] private Transform _inventoryItemViewsContainer;

        private PoolManager _poolManager;

        private Inventory _inventory;
        public Inventory Inventory => _inventory;

        private GameplayContext _context;
        public GameplayContext Context => _context;

        private Vector2 _slotSize;
        public Vector2 SlotSize => _slotSize;

        private Dictionary<Vector2Int, InventorySlotView> _inventorySlotViews;
        private Dictionary<string, InventoryItemView> _inventoryItemViews;

        public IReadOnlyDictionary<Vector2Int, InventorySlotView> InventorySlotViews => _inventorySlotViews;

        public void Setup(Inventory inventory, GameplayContext context)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _inventory = inventory;
            _context = context;

            // Setup slot and item views
            _inventorySlotViews = CreateInventorySlotViews();
            _inventoryItemViews = new();

            OnRectTransformDimensionsChange();

            foreach (KeyValuePair<string, InventoryItem> kvp in _inventory.InventoryItems)
            {
                HandleInventoryItemChanged(kvp.Key, null, kvp.Value);
            }

            _inventory.OnInventoryItemChanged += HandleInventoryItemChanged;

            RefreshSlotViewOutlines();
        }

        private void OnDestroy()
        {
            // Return pooled views
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

                view.Setup(this, new Vector2Int(kvp.Key.x, kvp.Key.y));

                views.Add(cell, view);
            }

            return views;
        }

        private void HandleInventoryItemChanged(string instanceId, InventoryItem oldInventoryItem, InventoryItem newInventoryItem)
        {
            if (newInventoryItem != null)
            {
                SetInventoryItemToSlotViews(newInventoryItem);
                SetInventoryItemView(instanceId, newInventoryItem);
            }
            else
            {
                RemoveInventoryItemFromSlotViews(oldInventoryItem);
                RemoveInventoryItemView(instanceId);
            }

            RefreshSlotViewOutlines();
        }

        private void SetInventoryItemToSlotViews(InventoryItem inventoryItem)
        {
            foreach (KeyValuePair<Vector2Int, bool> kvp in inventoryItem.Shape)
            {
                if (kvp.Value)
                {
                    _inventorySlotViews[inventoryItem.Pivot + kvp.Key].SetInventoryItem(inventoryItem);
                }
            }
        }

        private void RemoveInventoryItemFromSlotViews(InventoryItem inventoryItem)
        {
            foreach (KeyValuePair<Vector2Int, bool> kvp in inventoryItem.Shape)
            {
                if (kvp.Value)
                {
                    _inventorySlotViews[inventoryItem.Pivot + kvp.Key].SetInventoryItem(null);
                }
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
                Log.Error(this, "Tried to remove a view that does not exist");
                return;
            }

            _poolManager.Return(_inventoryItemViews[key]);
            _inventoryItemViews.Remove(key);
        }

        private void RefreshSlotViewOutlines()
        {
            foreach (InventorySlotView slotView in _inventorySlotViews.Values)
            {
                slotView.RefreshOutline();
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_inventory == null)
            {
                return;
            }

            RecalculateSlotSize();

            UpdateSlotViewTransforms();

            // Size affects many things for an ItemView, so a full update is fine here
            UpdateItemViews();
        }

        private void RecalculateSlotSize()
        {
            _slotSize = new Vector2(_rectTransform.rect.width / _inventory.Columns, _rectTransform.rect.height / _inventory.Rows);
        }

        private void UpdateSlotViewTransforms()
        {
            // Since cells are only positive, we need to use a pivot to center them in the widget
            float pivotX = (_inventory.Columns - 1) / 2f;
            float pivotY = (_inventory.Rows - 1) / 2f;

            foreach (KeyValuePair<Vector2Int, InventorySlotView> kvp in _inventorySlotViews)
            {
                Vector2 position = new Vector2(
                    (kvp.Key.x - pivotX) * _slotSize.x,
                    (kvp.Key.y - pivotY) * _slotSize.y);

                kvp.Value.SetTransform(position, _slotSize);
            }
        }

        private void UpdateItemViews()
        {
            foreach (InventoryItemView view in _inventoryItemViews.Values)
            {
                view.View.SetSlotSize(_slotSize);
                view.UpdateView();
            }
        }
    }
}