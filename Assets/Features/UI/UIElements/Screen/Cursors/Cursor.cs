using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.Items;
using FishFlingers.Pools;
using FishFlingers.States;
using ShinyOwl.Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class Cursor : MonoBehaviour, IPoolable
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _itemImage;
        [SerializeField] private Image _handImage;

        private ItemManager _itemManager;
        private UIManager _uiManager;

        public RectTransform RectTransform => _rectTransform;

        private RaftPlayer _owner;

        private PointerEventData _pointerEventData;
        private List<RaycastResult> _raycastResults = new();

        private static readonly Vector2 DefaultSize = Vector2.one * 75f;
        private const float ScaleSpeed = 25f;

        private void Awake()
        {
            _itemManager = GameManager.Instance.Get<ItemManager>();
            _uiManager = GameManager.Instance.Get<UIManager>();
        }

        private void Start()
        {
            _pointerEventData = new PointerEventData(EventSystem.current);
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
            Vector2 targetSize = inventoryWidget != null ? inventoryWidget.SlotSize : DefaultSize;
            _rectTransform.sizeDelta = Vector2.Lerp(_rectTransform.sizeDelta, targetSize, ScaleSpeed * Time.deltaTime);
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

            HandleHeldItemChanged(_owner?.NetHeldInventoryItem);

            // No need to show the hand image for the local client
            _handImage.gameObject.SetActive(!_owner?.IsLocalPlayer ?? false);
        }

        private void HandleHeldItemChanged(NetInventoryItem item)
        {
            _itemImage.sprite = item != null ? _itemManager.GetItemData(item.ItemId).Sprite : null;
            _itemImage.gameObject.SetActive(item != null);
        }

        public void OnReturnedToPool()
        {
            SetOwner(null);
        }

        public void OnTakenFromPool()
        { }
    }
}