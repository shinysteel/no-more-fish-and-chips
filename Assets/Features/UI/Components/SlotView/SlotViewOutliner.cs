using FishFlingers.Inventories;
using FishFlingers.States;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

namespace FishFlingers.UI
{
    public class SlotViewOutliner<T> where T : Component, ISlotView
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
            _context.LocalPlayer.HeldItemLogic.OnChanged += HandleHeldItemChanged;
        }

        ~SlotViewOutliner()
        {
            if (_context.LocalPlayer != null)
            {
                _context.LocalPlayer.HeldItemLogic.OnChanged -= HandleHeldItemChanged;
            }
        }

        private void HandleHeldItemChanged(InventoryItem item)
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
        {

        }
    }
}