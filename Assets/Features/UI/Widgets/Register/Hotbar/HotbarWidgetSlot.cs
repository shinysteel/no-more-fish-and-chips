using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using PurrLobby;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class HotbarWidgetSlot : RegisterSlotView, ITypedPoolable
    {
        [SerializeField] private Image _assignmentImage;

        private int _index = -1;
        public int Index => _index;

        public void SetIndex(int index)
        {
            _index = index;

            _cellOutline.SetColor(CellOutline.EColor.Default);
            _cellOutline.SetEnabled(true, true, true, true);
        }

        public override void SetInventoryItem(InventoryItem item)
        {
            base.SetInventoryItem(item);
            
            RefreshAssignmentImage();
        }

        private void RefreshAssignmentImage()
        {
            if (_inventoryItem != null && _context.LocalPlayer.Hotbar.IsItemAssigned(_inventoryItem, out int index))
            {
                _assignmentImage.sprite = _itemManager.GetAssignmentSprite(index);
                _assignmentImage.enabled = true;
            }
            else
            {
                _assignmentImage.enabled = false;
            }
        }
    }
}