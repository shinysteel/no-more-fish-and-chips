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
        [SerializeField] private RectTransform _cellOutlinesContainer;

        private PoolManager _poolManager;

        private InventoryItem _inventoryItem;

        public RectTransform RectTransform => _rectTransform;
        public InventoryItem InventoryItem => _inventoryItem;

        private static readonly Vector2 DefaultSlotSize = new Vector2(60, 60);
        private Vector2 _slotSize = DefaultSlotSize;


        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public void Setup(InventoryItem inventoryItem)
        {
            _inventoryItem = inventoryItem;

            UpdateView();
        }

        public void SetSlotSize(Vector2 size)
        {
            _slotSize = size;
        }

        private Vector2 CalculateAnchoredPositionForCell(Vector2Int cell)
        {
            // Offset relative to center, and respect inherited rotation
            Vector2 rawOffset = new Vector2(cell.x - _inventoryItem.Shape.GridBounds.center.x, cell.y - _inventoryItem.Shape.GridBounds.center.y);
            Vector2 rotatedOffset = Utils.Math.RotateCell(rawOffset, _inventoryItem.Rotations, false);

            Vector2 slotSize = _inventoryItem.Rotations % 2 == 0 ? _slotSize : new Vector2(_slotSize.y, _slotSize.x);

            return rotatedOffset * slotSize;
        }

        // View is implied, but the method Update is taken by Monobehaviour
        public void UpdateView()
        {
            if (_inventoryItem == null)
            {
                return;
            }

            UpdateImage();
            UpdateCount();
        }

        private void UpdateImage()
        {
            bool horizontal = _inventoryItem.Rotations % 2 == 0;
            int columns = horizontal ? _inventoryItem.Shape.Columns : _inventoryItem.Shape.Rows;
            int rows = horizontal ? _inventoryItem.Shape.Rows : _inventoryItem.Shape.Columns;

            float sizeX = horizontal ? _slotSize.x : _slotSize.y;
            float sizeY = horizontal ? _slotSize.y : _slotSize.x;

            // Size
            _rectTransform.sizeDelta = new Vector2(sizeX * columns, sizeY * rows);

            // Rotation, negative Z is clockwise
            _rectTransform.eulerAngles = new Vector3(0f, 0f, _inventoryItem.Rotations * -90f);

            // Sprite
            _image.sprite = _inventoryItem.ItemInstance.Data.Sprite;
        }

        private void UpdateCount()
        {
            // Size
            _countText.rectTransform.sizeDelta = _slotSize;

            // Count
            _countText.text = _inventoryItem.ItemInstance.Count.ToString();

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