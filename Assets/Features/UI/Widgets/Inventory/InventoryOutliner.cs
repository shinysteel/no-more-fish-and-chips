using FishFlingers.Inventories;
using FishFlingers.States;
using FishFlingers.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ShinyOwl.Common;

namespace FishFlingers.UI
{
    // Keeps slot view outlines up to date in an InventoryWidget
    public class InventoryOutliner
    {
        private UIManager _uiManager;

        private InventoryWidget _inventoryWidget;

        private PointerEventData _pointerEventData;
        private List<RaycastResult> _raycastResults = new();

        private InventorySlotView _targetSlotView;

        public InventoryOutliner(InventoryWidget widget)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();

            _inventoryWidget = widget;

            _pointerEventData = new PointerEventData(EventSystem.current);

            _inventoryWidget.Context.LocalPlayer.HeldItemLogic.OnChanged += HandleHeldItemChanged;

            Refresh();
        }

        ~InventoryOutliner()
        {
            _inventoryWidget.Context.LocalPlayer.HeldItemLogic.OnChanged -= HandleHeldItemChanged;
        }

        private void HandleHeldItemChanged(InventoryItem item)
        {
            Refresh();
        }

        public void Tick()
        {
            TargetSlotViewTick();
        }

        // _targetSlotView represents any InventorySlotView under the mouse
        private void TargetSlotViewTick()
        {
            _pointerEventData.Reset();
            _pointerEventData.position = Input.mousePosition;
            _raycastResults.Clear();

            _uiManager.ScreenGraphicRaycaster.Raycast(_pointerEventData, _raycastResults);

            InventorySlotView slotView = null;

            foreach (RaycastResult result in _raycastResults)
            {
                if (result.gameObject.TryGetComponent(out slotView))
                {
                    break;
                }
            }

            if (_targetSlotView == slotView)
            {
                return;
            }

            _targetSlotView = slotView;

            Refresh();
        }

        public void Refresh()
        {
            RefreshColor();
            RefreshEnabled();
        }
        
        // Change slot view outline color based on context
        private void RefreshColor()
        {
            // By default, all cells are grey
            foreach (InventorySlotView slotView in _inventoryWidget.InventorySlotViews.Values)
            {
                slotView.CellOutline.SetColor(Color.gray);
            }

            if (_targetSlotView == null)
            {
                return;
            }

            InventoryItem heldInventoryItem = _inventoryWidget.Context.LocalPlayer.HeldItemLogic.HeldInventoryItem;
            if (heldInventoryItem == null)
            {
                // Color the target item white
                if (_targetSlotView.InventoryItem != null)
                {
                    _targetSlotView.InventoryItem.Shape.ForEachTrue((Vector2Int cell) =>
                    {
                        _inventoryWidget.InventorySlotViews[_targetSlotView.InventoryItem.Pivot + cell].CellOutline.SetColor(Color.white);
                    });
                }
            }
            else
            {
                // Color the potential place action green or red
                Color color = true
                    ? Color.green
                    : Color.red;

                heldInventoryItem.Shape.ForEachTrue((Vector2Int cell) =>
                {
                    if (_inventoryWidget.InventorySlotViews.TryGetValue(_targetSlotView.Cell + cell, out InventorySlotView slotView))
                    {
                        slotView.CellOutline.SetColor(Color.green);
                    }
                });
            }
        }

        // When the enabled state of outlines are dirty, we need a refresh
        private void RefreshEnabled()
        {
            // By default, all sides are enabled
            foreach (InventorySlotView slotView in _inventoryWidget.InventorySlotViews.Values)
            {
                slotView.CellOutline.SetEnabled(true, true, true, true);
            }

            // Enables only the perimeter of an item as if it existed at the given pivot
            void EnablePerimeter(InventoryItem item, Vector2Int pivot)
            {
                item.Shape.ForEachTrue((Vector2Int cell) =>
                {
                    if (!_inventoryWidget.InventorySlotViews.TryGetValue(pivot + cell, out InventorySlotView slotView))
                    {
                        return;
                    }
                    
                    bool top = !item.Shape.TryGetBool(cell + Vector2Int.up, out _);
                    bool left = !item.Shape.TryGetBool(cell + Vector2Int.left, out _);
                    bool bottom = !item.Shape.TryGetBool(cell + Vector2Int.down, out _);
                    bool right = !item.Shape.TryGetBool(cell + Vector2Int.right, out _);

                    slotView.CellOutline.SetEnabled(top, left, bottom, right);
                });
            }

            // Items only enable their 'perimeter'
            foreach (InventoryItemView itemView in _inventoryWidget.InventoryItemViews.Values)
            {
                EnablePerimeter(itemView.View.InventoryItem, itemView.View.InventoryItem.Pivot);
            }

            InventoryItem heldInventoryItem = _inventoryWidget.Context.LocalPlayer.HeldItemLogic.HeldInventoryItem;
            if (heldInventoryItem == null || _targetSlotView == null)
            {   
                return;
            }

            EnablePerimeter(heldInventoryItem, _targetSlotView.Cell);
        }
    }
}