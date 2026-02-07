using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.Items;
using FishFlingers.Pools;
using FishFlingers.States;
using ShinyOwl.Common;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class Cursor : MonoBehaviour, IPoolable
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _itemImage;
        [SerializeField] private Image _handImage;

        private ItemManager _itemManager;

        public RectTransform RectTransform => _rectTransform;

        private RaftPlayer _owner;

        private void Awake()
        {
            _itemManager = GameManager.Instance.Get<ItemManager>();
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

            HandleHeldItemChanged(_owner?.HeldInventoryItem);

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