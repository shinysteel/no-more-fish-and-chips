using FishFlingers.Inventories;
using FishFlingers.States;
using PurrLobby;
using ShinyOwl.Common;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
namespace FishFlingers.UI
{
    public interface ISlotView
    { }

    public class SlotView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _image;
        [SerializeField] private CellOutline _cellOutline;

        public RectTransform RectTransform => _rectTransform;
        public CellOutline CellOutline => _cellOutline;

        private int _hotbarIndex = -1;

        private InventoryItem _inventoryItem;
        public InventoryItem InventoryItem => _inventoryItem;

        [SerializeField] private Color _defaultColor;
        [SerializeField] private Color _itemColor;
        [SerializeField] private Color[] _hotbarColors;

        private GameplayContext _context;

        public void Setup(GameplayContext context)
        {
            _context = context;
            _context.LocalPlayer.Hotbar.OnSlotChanged += HandleHotbarSlotChanged;
        }

        public void OnDestroy()
        {
            if (_context.LocalPlayer != null)
            {
                _context.LocalPlayer.Hotbar.OnSlotChanged -= HandleHotbarSlotChanged;
            }

            _hotbarIndex = -1;
            _inventoryItem = null;
            RefreshColor();
        }

        private void HandleHotbarSlotChanged(int index, InventoryItem item)
        {
            if (_inventoryItem == null)
            {
                return;
            }

            int newIndex = _hotbarIndex;

            // If we are linked to the assigned item, keep the index up to date
            if (_inventoryItem.ItemInstance.InstanceId == item?.ItemInstance.InstanceId)
            {
                newIndex = index;
            }
            // If our linked item has become null, reflect that
            else if (_hotbarIndex == index)
            {
                newIndex = -1;
            }

            if (_hotbarIndex != newIndex)
            {
                _hotbarIndex = newIndex;
                RefreshColor();
            }
        }

        public void SetInventoryItem(InventoryItem item)
        {
            if (_inventoryItem == item)
            {
                return;
            }

            _inventoryItem = item;

            if (item != null)
            {
                _context.LocalPlayer.Hotbar.IsItemAssigned(item, out int index);
                HandleHotbarSlotChanged(index, item);
            }
            else
            {
                _hotbarIndex = -1;
            }

            RefreshColor();
        }

        public void RefreshColor()
        {
            Color color;

            if (_hotbarIndex >= 0)
            {
                color = _hotbarColors[_hotbarIndex];
            }
            else if (_inventoryItem != null)
            {
                color = _itemColor;
            }
            else
            {
                color = _defaultColor;
            }

            _image.color = color;
        }

        public void SetTransform(Vector2 position, Vector2 size)
        {
            _rectTransform.anchoredPosition = position;
            _rectTransform.sizeDelta = size;
        }
    }
}