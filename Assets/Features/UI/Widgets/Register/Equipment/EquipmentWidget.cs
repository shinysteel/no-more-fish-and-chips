using UnityEngine;
using System.Threading.Tasks;
using UnityEditor.Search;
using NoMoreFishAndChips.Pools;
using ShinyOwl.Common;

namespace NoMoreFishAndChips.UI
{
    public class EquipmentWidget : RegisterWidget<EquipmentSlotView>
    {
        protected override EquipmentSlotView[] CreateSlots()
        {
            EquipmentSlotView[] slots = new EquipmentSlotView[2];

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = _poolManager.GetTypedPoolable<EquipmentSlotView>(new SpawnParams() { Parent = _slotsRectTransform });
                slots[i].Setup(_context);
            }

            return slots;
        }
    }
}