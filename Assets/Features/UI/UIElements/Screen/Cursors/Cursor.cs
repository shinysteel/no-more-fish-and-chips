using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class Cursor : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private GameObject _visualsGameObject;
        [SerializeField] private ItemView _itemView;
        [SerializeField] private Image _handImage;
        [SerializeField] private ItemInfo _itemInfo;

        private UIManager _uiManager;

        private GameplayContext _context;

        private RaftPlayer _owner;
        public RaftPlayer Owner => _owner;

        private PointerEventData _pointerEventData;
        private List<RaycastResult> _raycastResults = new();

        private ItemDefinitionData _currentItemDefinitionData;

        private Tween _resizeTween;
        private Vector2 _targetSlotSize;

        private const float ResizeDuration = 0.1f;
        private static readonly Vector2 DefaultSlotSize = Vector2.one * 75f;

        public RectTransform RectTransform => _rectTransform;

        private void Awake()
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
        }

        public void Setup(GameplayContext context)
        {
            _context = context;
        }

        private void Start()
        {
            _pointerEventData = new PointerEventData(EventSystem.current);

            _targetSlotSize = DefaultSlotSize;
            Resize(_targetSlotSize);

            RefreshInfo();
        }

        private void Update()
        {
            RaycastUpdate();
            SizeUpdate();

            if (_owner.isOwner)
            {
                InfoUpdate();
            }
        }

        private void RaycastUpdate()
        {
            _pointerEventData.Reset();
            _pointerEventData.position = RectTransformUtility.WorldToScreenPoint(null, _rectTransform.position);

            _raycastResults.Clear();

            _uiManager.ScreenGraphicRaycaster.Raycast(_pointerEventData, _raycastResults);
        }

        private void SizeUpdate()
        {
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
            _resizeTween.Stop();

            _resizeTween = Tween.Custom(startValue: _targetSlotSize, endValue: newTargetSize, duration: ResizeDuration, onValueChange: Resize);

            _targetSlotSize = newTargetSize;
        }

        private void InfoUpdate()
        {
            SlotView view = null;

            foreach (RaycastResult result in _raycastResults)
            {
                if (result.gameObject.TryGetComponent(out view))
                {
                    break;
                }
            }

            if (_currentItemDefinitionData == view?.InventoryItem?.ItemInstance.Data)
            {
                return;
            }

            _currentItemDefinitionData = view?.InventoryItem?.ItemInstance.Data;
            RefreshInfo();
        }

        private void RefreshInfo()
        {
            if (_currentItemDefinitionData == null)
            {
                _itemInfo.gameObject.SetActive(false);
            }
            else
            {
                _itemInfo.Set(_currentItemDefinitionData);
                _itemInfo.gameObject.SetActive(true);
            }
        }

        private void Resize(Vector2 slotSize)
        {
            if (_itemView.gameObject.activeSelf)
            {
                _itemView.SetSlotSize(slotSize);
                _itemView.Refresh();
            }

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
                _owner.GrabbedInventoryItemLogic.OnGrabbedInventoryItemChanged -= HandleGrabbedInventoryItemChanged;
            }

            _owner = owner;

            if (_owner != null)
            {
                _owner.GrabbedInventoryItemLogic.OnGrabbedInventoryItemChanged += HandleGrabbedInventoryItemChanged;
            }

            HandleGrabbedInventoryItemChanged(_owner?.GrabbedInventoryItemLogic.GrabbedInventoryItem);

            // No need to show the hand image for the local client
            _handImage.gameObject.SetActive(!_owner?.IsLocalPlayer ?? false);
        }

        private void HandleGrabbedInventoryItemChanged(InventoryItem item)
        {
            if (item != null)
            {
                _itemView.Setup(_context, item);
                _itemView.gameObject.SetActive(true);
            }
            else
            {
                _itemView.gameObject.SetActive(false);
            }
        }

        public void SetVisualsActive(bool active)
        {
            _visualsGameObject.SetActive(active);
        }

        public void OnReturnedToPool()
        {
            SetOwner(null);
        }

        public void OnTakenFromPool()
        { }
    }
}