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
            InventoryItem heldItem = _context.LocalPlayer.HeldItemLogic.HeldInventoryItem;

            foreach (HotbarWidgetSlot slot in _hotbarWidget.Slots)
            {
                CellOutline.EColor color;

                if (heldItem != null && slot == _targetSlotView)
                {
                    color = CellOutline.EColor.Positive;
                }
                else if (heldItem == null && slot == _targetSlotView && slot.InventoryItem != null)
                {
                    color = CellOutline.EColor.Highlighted;
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