using FishFlingers.Inventories;
using FishFlingers.States;
using FishFlingers.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ShinyOwl.Common;
using FishFlingers.Entities;

namespace FishFlingers.UI
{
    // Keeps slot view outlines up to date in an InventoryWidget
    public class InventoryOutliner : SlotViewOutliner<InventorySlotView>
    {
        private InventoryWidget _inventoryWidget;

        public InventoryOutliner(GameplayContext context, InventoryWidget widget) : base(context)
        {
            _inventoryWidget = widget;

            Refresh();
        }

        public override void Refresh()
        {
            RefreshColors();
            RefreshEnabled();
        }
        
        // Change slot view outline color based on context
        private void RefreshColors()
        {
            // By default, all cells are grey
            foreach (InventorySlotView slotView in _inventoryWidget.InventorySlotViews.Values)
            {
                slotView.CellOutline.SetColor(CellOutline.EColor.Default);
            }

            if (_targetSlotView == null)
            {
                return;
            }
            
            if (_targetSlotView.InventoryWidget != _inventoryWidget)
            {
                return;
            }

            InventoryItem grabbedInventoryItem = _context.LocalPlayer.GrabbedInventoryItemLogic.GrabbedInventoryItem;

            if (grabbedInventoryItem == null)
            {
                if (_targetSlotView.InventoryItem != null)
                {
                    // Color the target item white or red
                    CellOutline.EColor color = _targetSlotView.InventoryItem.IsGrabbed == false 
                        ? CellOutline.EColor.Highlighted 
                        : CellOutline.EColor.Negative;

                    _targetSlotView.InventoryItem.Shape.ForEachTrue((Vector2Int cell) =>
                    {
                        _inventoryWidget.InventorySlotViews[_targetSlotView.InventoryItem.Cell + cell].CellOutline.SetColor(color);
                    });
                }
            }
            else
            {
                // Color the potential place action green or red
                CellOutline.EColor color = _inventoryWidget.Inventory.CanPlaceItem(InventoryPlaceParams.Create(_targetSlotView.Cell, grabbedInventoryItem), out _, out _, out _)
                    ? CellOutline.EColor.Positive
                    : CellOutline.EColor.Negative;

                grabbedInventoryItem.Shape.ForEachTrue((Vector2Int cell) =>
                {
                    if (_inventoryWidget.InventorySlotViews.TryGetValue(_targetSlotView.Cell + cell, out InventorySlotView slotView))
                    {
                        slotView.CellOutline.SetColor(color);
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
            void EnablePerimeter(InventoryItem item, Vector2Int itemCell)
            {
                item.Shape.ForEachTrue((Vector2Int shapeCell) =>
                {
                    if (!_inventoryWidget.InventorySlotViews.TryGetValue(itemCell + shapeCell, out InventorySlotView slotView))
                    {
                        return;
                    }
                    
                    bool top = !item.Shape.TryGetBool(shapeCell + Vector2Int.up, out bool topBool) || !topBool;
                    bool left = !item.Shape.TryGetBool(shapeCell + Vector2Int.left, out bool leftBool) || !leftBool;
                    bool bottom = !item.Shape.TryGetBool(shapeCell + Vector2Int.down, out bool bottomBool) || !bottomBool;
                    bool right = !item.Shape.TryGetBool(shapeCell + Vector2Int.right, out bool rightBool) || !rightBool;

                    slotView.CellOutline.SetEnabled(top, left, bottom, right);
                });
            }

            // Items only enable their 'perimeter'
            foreach (InventoryItemView itemView in _inventoryWidget.InventoryItemViews.Values)
            {
                EnablePerimeter(itemView.InventoryItem, itemView.InventoryItem.Cell);
            }

            if (_targetSlotView == null)
            {
                return;
            }

            if (_targetSlotView.InventoryWidget != _inventoryWidget)
            {
                return;
            }

            InventoryItem grabbedInventoryItem = _context.LocalPlayer.GrabbedInventoryItemLogic.GrabbedInventoryItem;

            if (grabbedInventoryItem == null)
            {
                return;
            }

            EnablePerimeter(grabbedInventoryItem, _targetSlotView.Cell);
        }
    }
}