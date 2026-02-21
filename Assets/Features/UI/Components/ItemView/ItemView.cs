using FishFlingers.Inventories;
using FishFlingers.Pools;
using FishFlingers.States;
using NUnit.Framework;
using PurrLobby;
using ShinyOwl.Common;
using ShinyOwl.Common.Structures;
using ShinyOwl.Common.Utils;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace FishFlingers.UI
{
    public class ItemView : MonoBehaviour 
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _countText;

        private InventoryItem _inventoryItem;

        public RectTransform RectTransform => _rectTransform;
        public InventoryItem InventoryItem => _inventoryItem;

        private static readonly Vector2 DefaultSlotSize = new Vector2(60, 60);
        private Vector2 _slotSize = DefaultSlotSize;

        public Vector2 SlotSize => _slotSize;

        public void Setup(InventoryItem item)
        {
            SetInventoryItem(item);

            Refresh();
        }

        public void SetInventoryItem(InventoryItem item)
        {
            _inventoryItem = item;
        }

        public void SetSlotSize(Vector2 size)
        {
            _slotSize = size;
        }

        public void ResetAlpha()
        {
            SetAlpha(1f);
        }

        public void SetAlpha(float alpha)
        {
            _image.color = new Color(1f, 1f, 1f, alpha);
        }

        private Vector2 CalculateAnchoredPositionForCell(Vector2Int cell)
        {
            // Offset relative to center, and respect inherited rotation
            Vector2 rawOffset = new Vector2(cell.x - _inventoryItem.Shape.GridBounds.center.x, cell.y - _inventoryItem.Shape.GridBounds.center.y);
            Vector2 rotatedOffset = Utils.Math.RotateCell(rawOffset, _inventoryItem.Rotations, false);

            Vector2 slotSize = _inventoryItem.Rotations % 2 == 0 ? _slotSize : new Vector2(_slotSize.y, _slotSize.x);

            return rotatedOffset * slotSize;
        }

        public void Refresh()
        {
            if (_inventoryItem == null)
            {
                return;
            }

            RefreshRect();
            RefreshImage();
            RefreshCount();
        }

        public void RefreshRect()
        {
            bool horizontal = _inventoryItem.Rotations % 2 == 0;

            int columns = horizontal ? _inventoryItem.Shape.Columns : _inventoryItem.Shape.Rows;
            int rows = horizontal ? _inventoryItem.Shape.Rows : _inventoryItem.Shape.Columns;

            float sizeX = horizontal ? _slotSize.x : _slotSize.y;
            float sizeY = horizontal ? _slotSize.y : _slotSize.x;

            int pivotX = horizontal ? _inventoryItem.Pivot.x : _inventoryItem.Pivot.y;
            int pivotY = horizontal ? _inventoryItem.Pivot.y : _inventoryItem.Pivot.x;

            int minX = horizontal ? _inventoryItem.Shape.TrueBounds.xMin : _inventoryItem.Shape.TrueBounds.yMin;
            int minY = horizontal ? _inventoryItem.Shape.TrueBounds.yMin : _inventoryItem.Shape.TrueBounds.xMin;

            Vector2 pivot = new Vector2((-minX + 0.5f) / columns, (-minY + 0.5f) / rows);

            pivot = _inventoryItem.Rotations switch
            {
                0 => pivot,
                1 => new Vector2(1f - pivot.x, pivot.y),
                2 => Vector2Int.one - pivot,
                3 => new Vector2(pivot.x, 1f - pivot.y),
                _ => pivot
            };

            // Pivot
            _rectTransform.pivot = pivot;

            // Size
            _rectTransform.sizeDelta = new Vector2(sizeX * columns, sizeY * rows);

            // Rotation, negative Z is clockwise
            _rectTransform.eulerAngles = new Vector3(0f, 0f, _inventoryItem.Rotations * -90f);
        }

        public void RefreshImage()
        {
            // Sprite
            _image.sprite = _inventoryItem.ItemInstance.Data.Sprite;
        }

        public void RefreshCount()
        {
            RefreshCountText();
            RefreshCountRect();
        }

        public void RefreshCountText()
        {
            // Size
            _countText.rectTransform.sizeDelta = _slotSize;

            // Count
            _countText.text = _inventoryItem.ItemInstance.Count.ToString();
        }

        public void RefreshCountRect()
        {
            // Sort by bottom, then rightmost
            Vector2Int cell = _inventoryItem.Shape
                .Where(kvp => kvp.Value == true)
                .OrderBy(kvp => kvp.Key.y)
                .ThenByDescending(kvp => kvp.Key.x)
                .First()
                .Key;

            // Position
            _countText.rectTransform.anchoredPosition = CalculateAnchoredPositionForCell(cell);

            // Rotation
            _countText.rectTransform.eulerAngles = Vector3.zero;
        }
    }
}