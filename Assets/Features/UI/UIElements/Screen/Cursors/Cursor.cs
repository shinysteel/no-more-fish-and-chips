using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.Items;
using FishFlingers.Pools;
using FishFlingers.States;
using PrimeTween;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class Cursor : MonoBehaviour, IPoolable
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private ItemView _itemView;
        [SerializeField] private Image _handImage;

        private UIManager _uiManager;

        private RaftPlayer _owner;

        private PointerEventData _pointerEventData;
        private List<RaycastResult> _raycastResults = new();

        private Tween _resizeTween;
        private Vector2 _targetSlotSize;

        private const float ResizeDuration = 0.1f;
        private static readonly Vector2 DefaultSlotSize = Vector2.one * 75f;

        public RectTransform RectTransform => _rectTransform;

        private void Awake()
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
        }

        private void Start()
        {
            _pointerEventData = new PointerEventData(EventSystem.current);

            _targetSlotSize = DefaultSlotSize;
            Resize(_targetSlotSize);
        }

        private void Update()
        {
            SizeUpdate();
        }

        private void SizeUpdate()
        {
            _pointerEventData.Reset();
            _pointerEventData.position = RectTransformUtility.WorldToScreenPoint(null, _rectTransform.position);

            _raycastResults.Clear();

            _uiManager.ScreenGraphicRaycaster.Raycast(_pointerEventData, _raycastResults);

            InventoryWidget inventoryWidget = null;

            // Check if the cursor is overlapping an inventory widget
            foreach (RaycastResult result in _raycastResults)
            {
                if (result.gameObject.TryGetComponent(out inventoryWidget))
                {
                    break;
                }
            }

            // Scale adapts to what the cursor is over
            Vector2 newTargetSize = inventoryWidget != null ? inventoryWidget.SlotSize : DefaultSlotSize;

            if (_targetSlotSize == newTargetSize)
            {
                return;
            }

            // One scale tween active at a time
            if (_resizeTween.isAlive)
            {
                _resizeTween.Stop();
            }

            _resizeTween = Tween.Custom(startValue: _targetSlotSize, endValue: newTargetSize, duration: ResizeDuration, onValueChange: Resize);

            _targetSlotSize = newTargetSize;
        }

        private void Resize(Vector2 slotSize)
        {
            _itemView.SetSlotSize(slotSize);
            _itemView.Refresh();
            _handImage.rectTransform.sizeDelta = slotSize * 0.9f;
        }

        public void SetOwner(RaftPlayer owner)
        {
            if (_owner == owner)
            {
                return;
            }
            
            if (_owner != null)
            {
                _owner.HeldItemLogic.OnChanged -= HandleHeldItemChanged;
            }

            _owner = owner;

            if (_owner != null)
            {
                _owner.HeldItemLogic.OnChanged += HandleHeldItemChanged;
            }

            HandleHeldItemChanged(_owner?.HeldItemLogic.HeldInventoryItem);

            // No need to show the hand image for the local client
            _handImage.gameObject.SetActive(!_owner?.IsLocalPlayer ?? false);
        }

        private void HandleHeldItemChanged(InventoryItem item)
        {
            if (item != null)
            {
                _itemView.Setup(item);
                _itemView.gameObject.SetActive(true);
            }
            else
            {
                _itemView.gameObject.SetActive(false);
            }
        }

        public void OnReturnedToPool()
        {
            SetOwner(null);
        }

        public void OnTakenFromPool()
        { }
    }
}