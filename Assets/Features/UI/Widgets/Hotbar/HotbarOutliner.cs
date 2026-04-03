using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.States;
using UnityEngine;

namespace FishFlingers.UI
{
    public class HotbarOutliner : SlotViewOutliner<HotbarWidgetSlot>
    {
        private HotbarWidget _hotbarWidget;

        public HotbarOutliner(GameplayContext context, HotbarWidget widget) : base(context)
        {
            _hotbarWidget = widget;
        }

        public override void Refresh()
        {
            RefreshColors();
        }

        private void RefreshColors()
        {
            InventoryItem grabbedInventoryItem = _context.LocalPlayer.GrabbedInventoryItemLogic.GrabbedInventoryItem;

            foreach (HotbarWidgetSlot slot in _hotbarWidget.Slots)
            {
                CellOutline.EColor color;

                if (grabbedInventoryItem != null && slot == _targetSlotView)
                {
                    color = CellOutline.EColor.Positive;
                }
                else if (grabbedInventoryItem == null && slot == _targetSlotView && slot.InventoryItem != null)
                {
                    color = CellOutline.EColor.Negative;
                }
                else
                {
                    color = CellOutline.EColor.Default;
                }

                slot.CellOutline.SetColor(color);
            }
        }
    }
}