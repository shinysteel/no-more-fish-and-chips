using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace NoMoreFishAndChips.UI
{
    /// <summary>
    /// Handles raycasts for views relevant to an InventoryWidget
    /// </summary>
    public class InventoryRaycaster
    {
        private UIManager _uiManager;

        private PointerEventData _pointerEventData;
        private List<RaycastResult> _raycastResults = new();

        public InventoryRaycaster()
        {
            _uiManager = GameManager.Instance.Get<UIManager>();

            _pointerEventData = new PointerEventData(EventSystem.current);
        }

        /// <summary>
        /// Retrieve relevant views to target under the cursor
        /// </summary>
        public void GetViews(out InventoryItemView itemView, out InventorySlotView inventorySlot, out HotbarWidgetSlot hotbarSlot, out Panel panel)
        {
            itemView = null;
            inventorySlot = null;
            hotbarSlot = null;
            panel = null;

            List<InventoryItemView> itemViews = ListPool<InventoryItemView>.Get();

            _pointerEventData.Reset();
            _pointerEventData.position = Input.mousePosition;

            _raycastResults.Clear();

            _uiManager.ScreenGraphicRaycaster.Raycast(_pointerEventData, _raycastResults);

            // Retrieve the first inventory slot and hotbar slot we detect. We can expect multiple items in a single raycast,
            // so we use a list to track those
            foreach (RaycastResult result in _raycastResults)
            {
                if (result.gameObject.TryGetComponent(out itemView))
                {
                    itemViews.Add(itemView);
                }

                if (inventorySlot == null)
                {
                    result.gameObject.TryGetComponent(out inventorySlot);
                }

                if (hotbarSlot == null)
                {
                    result.gameObject.TryGetComponent(out hotbarSlot);
                }

                if (panel == null)
                {
                    result.gameObject.TryGetComponent(out panel);
                }
            }

            // Choose the most relevant itemView 
            try
            {
                if (itemViews.Count == 0)
                {
                    return;
                }

                itemView = itemViews[0];

                if (inventorySlot?.InventoryItem == null)
                {
                    return;
                }

                // Given items can overlap cells they aren't actually on, we'd prefer to target items that are actually on the slot
                for (int i = 0; i < itemViews.Count; i++)
                {
                    if (itemViews[i].InventoryItem.ItemInstance.InstanceId == inventorySlot.InventoryItem.ItemInstance.InstanceId)
                    {
                        itemView = itemViews[i];
                        return;
                    }
                }
            }
            finally
            {
                ListPool<InventoryItemView>.Release(itemViews);
            }
        }
    }
}