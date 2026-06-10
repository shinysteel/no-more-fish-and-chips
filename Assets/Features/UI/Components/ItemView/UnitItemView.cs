using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class UnitItemView : ItemView, ITypedPoolable
    {
        public override void Refresh()
        {
            // UnitItemView's verison of RefreshRect
            _rectTransform.sizeDelta = _slotSize;

            RefreshItemImage();
            RefreshCountText();
            RefreshAssignmentImage();

            // Omitted RefreshDetailsRects
        }

        public void OnReturnedToPool()
        {
            ResetAlpha();
        }

        public void OnTakenFromPool()
        { }
    }
}