using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.States;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using NoMoreFishAndChips.Entities;

namespace NoMoreFishAndChips.UI
{
    public class SlotViewOutliner<T> where T : SlotView
    {
        private UIManager _uiManager;

        protected GameplayContext _context;

        private PointerEventData _pointerEventData;
        private List<RaycastResult> _raycastResults;

        protected T _targetSlotView;

        public SlotViewOutliner(GameplayContext context)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();

            _pointerEventData = new PointerEventData(EventSystem.current);
            _raycastResults = new();

            _context = context;
            _context.LocalPlayer.GrabbedInventoryItemLogic.OnGrabbedInventoryItemChanged += HandleGrabbedInventoryItemChanged;
        }

        ~SlotViewOutliner()
        {
            if (_context.LocalPlayer != null)
            {
                _context.LocalPlayer.GrabbedInventoryItemLogic.OnGrabbedInventoryItemChanged -= HandleGrabbedInventoryItemChanged;
            }
        }

        private void HandleGrabbedInventoryItemChanged(InventoryItem item)
        {
            Refresh();
        }

        public void Tick()
        {
            DetermineTargetTick();
        }

        /// <summary>
        /// _targetSlotView represents any InventorySlotView under the mouse
        /// </summary>
        private void DetermineTargetTick()
        {
            _pointerEventData.Reset();
            _pointerEventData.position = Input.mousePosition;
            _raycastResults.Clear();

            _uiManager.ScreenGraphicRaycaster.Raycast(_pointerEventData, _raycastResults);

            T slotView = default;

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

        public virtual void Refresh()
        { }
    }
}