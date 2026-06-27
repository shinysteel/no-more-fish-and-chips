using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using PurrLobby;
using ShinyOwl.Common;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class SlotView : MonoBehaviour
    {
        [SerializeField] protected RectTransform _rectTransform;
        [SerializeField] protected CellOutline _cellOutline;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _lockedImage;

        [SerializeField] private Color _unlockedColor;
        [SerializeField] private Color _unlockableColor; 
        [SerializeField] private Color _lockedColor;
        
        protected PoolManager _poolManager;
        protected ItemManager _itemManager;

        protected GameplayContext _context;

        private int _hotbarIndex = -1;

        protected InventoryItem _inventoryItem;
        public InventoryItem InventoryItem => _inventoryItem;

        public RectTransform RectTransform => _rectTransform;
        public CellOutline CellOutline => _cellOutline;

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
            _itemManager = GameManager.Instance.Get<ItemManager>();
        }

        public void Setup(GameplayContext context)
        {
            _context = context;
            _context.LocalPlayer.Hotbar.OnSlotChanged += HandleHotbarSlotChanged;
        }

        protected void OnDestroy()
        {
            if (_context.LocalPlayer != null)
            {
                _context.LocalPlayer.Hotbar.OnSlotChanged -= HandleHotbarSlotChanged;
            }

            _hotbarIndex = -1;
            _inventoryItem = null;
        }

        private void HandleHotbarSlotChanged(HotbarSlot slot)
        {
            if (_inventoryItem == null)
            {
                return;
            }

            int newIndex = _hotbarIndex;

            // If we are linked to the assigned item, keep the index up to date
            if (_inventoryItem.ItemInstance.InstanceId == slot.InventoryItem?.ItemInstance.InstanceId)
            {
                newIndex = slot.Index;
            }
            // If our linked item has become null, reflect that
            else if (_hotbarIndex == slot.Index)
            {
                newIndex = -1;
            }

            if (_hotbarIndex != newIndex)
            {
                _hotbarIndex = newIndex;
            }
        }

        public virtual void SetInventoryItem(InventoryItem item)
        {
            if (_inventoryItem == item)
            {
                return;
            }

            _inventoryItem = item;

            if (item != null)
            {
                if (_context.LocalPlayer.Hotbar.IsItemAssigned(item, out int index))
                {
                    HandleHotbarSlotChanged(_context.LocalPlayer.Hotbar.Slots[index]);
                }
            }
            else
            {
                _hotbarIndex = -1;
            }
        }

        public virtual void SetTransform(Vector2 position, Vector2 size)
        {
            _rectTransform.anchoredPosition = position;
            _rectTransform.sizeDelta = size;
        }

        public void SetLockState(SlotLockState state)
        {
            _backgroundImage.color = state switch
            {
                SlotLockState.Unlocked => _unlockedColor,
                SlotLockState.Unlockable => _unlockableColor,
                SlotLockState.Locked => _lockedColor,
                _ => default
            };

            _lockedImage.gameObject.SetActive(state == SlotLockState.Unlockable);
        }
    }
}