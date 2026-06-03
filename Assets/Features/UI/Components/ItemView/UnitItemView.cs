using FishFlingers.Inventories;
using FishFlingers.Pools;
using FishFlingers.States;
using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.UI
{
    public class UnitItemView : MonoBehaviour, ITypedPoolable
    {
        // It's not safe to expose _view for this script's use case, so we will be routing methods through here
        [SerializeField] private ItemView _view;

        public RectTransform RectTransform => _view.RectTransform;

        public void Setup(GameplayContext context, InventoryItem item)
        {
            // Don't invoke Refresh through ItemView.Setup, since we don't want to inherit many parts of ItemView
            _view.SetContext(context);
            _view.SetInventoryItem(item);

            RefreshView();
        }

        public void SetSlotSize(Vector2 size)
        {
            _view.SetSlotSize(size);
        }

        public void RefreshView()
        {
            RefreshRect();

            if (_view.InventoryItem != null)
            {
                _view.RefreshItemImage();
                _view.RefreshCountText();
                _view.RefreshAssignmentImage();
            }
        }

        private void RefreshRect()
        {
            _view.RectTransform.sizeDelta = _view.SlotSize;
        }

        public void OnReturnedToPool()
        {
            _view.ResetAlpha();
        }

        public void OnTakenFromPool()
        { }
    }
}