using PurrNet;
using UnityEngine;
using FishFlingers.Environments;
using ShinyOwl.Common;
using FishFlingers.Items;

namespace FishFlingers.Entities
{
    public class DroppedItem : NetEntity, IInteractable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private SyncVar<ItemId> _itemId;
        private SyncVar<int> _count;

        public DroppedItemData Data => (DroppedItemData)_entityData;

        public Vector3 Position => transform.position;

        protected override void OnInitializeModules()
        {
            base.OnInitializeModules();

            _itemId = new();
            _count = new();
        }

        public void SetItem(ItemId itemId, int count)
        {
            _itemId.value = itemId;
            _count.value = count;

            _spriteRenderer.sprite = _itemManager.GetItemData(itemId).Sprite;
        }

        public void Interact()
        {
            if (_context.LocalPlayer.Inventory.TryAddItems(_itemId, _count))
            {
                _networkManager.Despawn(this);
            }
        }
    }
}