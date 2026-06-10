using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using ShinyOwl.Common.Structures;
using ShinyOwl.Common.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NoMoreFishAndChips.UI
{
    public class InventoryWidget : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Transform _inventorySlotViewsContainer;
        [SerializeField] private Transform _inventoryItemViewsContainer;

        private PoolManager _poolManager;

        private GameplayContext _context;

        private Inventory _inventory;
        public Inventory Inventory => _inventory;

        private Vector2 _slotSize;
        public Vector2 SlotSize => _slotSize;

        private Dictionary<Vector2Int, InventorySlotView> _inventorySlotViews;
        private Dictionary<string, InventoryItemView> _inventoryItemViews;

        public IReadOnlyDictionary<Vector2Int, InventorySlotView> InventorySlotViews => _inventorySlotViews;
        public IReadOnlyDictionary<string, InventoryItemView> InventoryItemViews => _inventoryItemViews;

        private InventoryOutliner _inventoryOutliner;

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public void Setup(GameplayContext context, Inventory inventory)
        {
            _context = context;
            _inventory = inventory;

            // Setup slot and item views
            _inventorySlotViews = CreateInventorySlotViews();
            _inventoryItemViews = new();

            OnRectTransformDimensionsChange();

            _inventoryOutliner = new InventoryOutliner(_context, this);

            foreach (KeyValuePair<string, InventoryItem> kvp in _inventory.InventoryItems)
            {
                HandleInventoryItemChanged(kvp.Key, null, kvp.Value);
            }

            _inventory.OnInventoryItemChanged += HandleInventoryItemChanged;
        }

        private void OnDestroy()
        {
            // Return pooled views
            if (_poolManager != null)
            {
                foreach (InventorySlotView view in _inventorySlotViews.Values)
                {
                    _poolManager.ReturnTypedPoolable(view);
                }

                foreach (InventoryItemView view in _inventoryItemViews.Values)
                {
                    _poolManager.ReturnTypedPoolable(view);
                }
            }

            if (_inventory != null)
            {
                _inventory.OnInventoryItemChanged -= HandleInventoryItemChanged;
            }
        }

        private void Update()
        {
            _inventoryOutliner.Tick();   
        }

        // Setup the slot views
        private Dictionary<Vector2Int, InventorySlotView> CreateInventorySlotViews()
        {
            Dictionary<Vector2Int, InventorySlotView> views = new();

            foreach (KeyValuePair<Vector2Int, NetInventorySlot> kvp in _inventory)
            {
                InventorySlotView view = _poolManager.GetTypedPoolable<InventorySlotView>(new SpawnParams() { Parent = _inventorySlotViewsContainer });

                Vector2Int cell = kvp.Key;

                view.Setup(_context);
                view.SetWidgetAndCell(this, new Vector2Int(kvp.Key.x, kvp.Key.y));

                views.Add(cell, view);
            }

            return views;
        }

        // Listen to item changes
        private void HandleInventoryItemChanged(string instanceId, InventoryItem oldInventoryItem, InventoryItem newInventoryItem)
        {
            SetInventoryItemToSlotViews(oldInventoryItem, newInventoryItem);

            if (newInventoryItem != null)
            {
                SetInventoryItemView(instanceId, newInventoryItem);
            }
            else
            {
                RemoveInventoryItemView(instanceId);
            }

            _inventoryOutliner.Refresh();
        }

        // Informs slot views on the status of items occupying them
        private void SetInventoryItemToSlotViews(InventoryItem oldInventoryItem, InventoryItem newInventoryItem)
        {
            void set(Vector2Int setCell, BoolGrid shape, InventoryItem item)
            {
                shape.ForEachTrue((Vector2Int shapeCell) =>
                {
                    _inventorySlotViews[setCell + shapeCell].SetInventoryItem(item);
                });
            }

            if (oldInventoryItem != null)
            {
                set(oldInventoryItem.Cell, oldInventoryItem.Shape, null);
            }

            if (newInventoryItem != null)
            {
                set(newInventoryItem.Cell, newInventoryItem.Shape, newInventoryItem);
            }
        }

        // Register an item to be displayed via an item view, and keep it up to date
        private void SetInventoryItemView(string key, InventoryItem inventoryItem)
        {
            if (!_inventoryItemViews.ContainsKey(key))
            {
                _inventoryItemViews[key] = _poolManager.GetTypedPoolable<InventoryItemView>(new SpawnParams() { Parent = _inventoryItemViewsContainer });   
            }
            
            InventoryItemView view = _inventoryItemViews[key];
            view.SetInventoryWidget(this);
            view.Setup(_context, inventoryItem);
        }

        private void RemoveInventoryItemView(string key)
        {
            if (!_inventoryItemViews.ContainsKey(key))
            {
                Log.Error("Tried to remove a view that does not exist");
                return;
            }

            _poolManager.ReturnTypedPoolable(_inventoryItemViews[key]);
            _inventoryItemViews.Remove(key);
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
                view.SetSlotSize(_slotSize);
                view.Refresh();
            }
        }
    }
}