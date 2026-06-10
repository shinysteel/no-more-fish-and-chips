using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using System.Threading.Tasks;

namespace NoMoreFishAndChips.UI
{
    public class HotbarWidget : RegisterWidget<HotbarWidgetSlot>
    {
        private Hotbar _hotbar;

        protected override HotbarWidgetSlot[] CreateSlots()
        {
            HotbarWidgetSlot[] slots = new HotbarWidgetSlot[_hotbar.Slots.Count];

            foreach (HotbarSlot slot in _hotbar.Slots)
            {
                slots[slot.Index] = _poolManager.GetTypedPoolable<HotbarWidgetSlot>(new SpawnParams() { Parent = _slotsRectTransform });
                slots[slot.Index].Setup(_context);
                slots[slot.Index].SetIndex(slot.Index);
            }

            return slots;
        }

        public override async Task SetupAsync(GameplayContext context)
        {
            // _context is not assigned yet
            _hotbar = context.LocalPlayer.Hotbar;

            await base.SetupAsync(context);

            foreach (HotbarSlot slot in _hotbar.Slots)
            {
                HandleSlotChanged(slot);
            }

            _hotbar.OnSlotChanged += HandleSlotChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_hotbar != null)
            {
                _hotbar.OnSlotChanged -= HandleSlotChanged;
            }
        }

        private void HandleSlotChanged(HotbarSlot slot)
        {
            _slots[slot.Index].SetInventoryItem(slot.InventoryItem);

            _outliner.Refresh();
        }
    }
}