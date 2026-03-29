using PurrNet;
using UnityEngine;
using FishFlingers.Environments;
using ShinyOwl.Common;
using FishFlingers.Items;
using FishFlingers.Inventories;
using Newtonsoft.Json;
using ShinyOwl.Common.Utils;
using System;

namespace FishFlingers.Entities
{
    public class DroppedItemSave
    {
        [JsonProperty] public string InstanceId { get; private set; }
        [JsonProperty] public ItemId ItemId { get; private set; }
        [JsonProperty] public int Count { get; private set; }

        [JsonProperty] private SimpleVector3 _position = new();

        [JsonIgnore] public Vector3 Position
        {
            get => _position.ToVector3();
            set => _position = new SimpleVector3(value);
        }

        private const int Precision = 1;

        public DroppedItemSave()
        { }
        
        public DroppedItemSave(DroppedItem droppedItem)
        {
            InstanceId = droppedItem.NetItemInstance.value.InstanceId;
            ItemId = droppedItem.NetItemInstance.value.ItemId;
            Count = droppedItem.NetItemInstance.value.Count;
            Position = Utils.Math.RoundVector3(droppedItem.transform.position, Precision);
        }
    }

    public class DroppedItem : NetEntity, IInteractable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private SyncVar<NetItemInstance> _netItemInstance = new SyncVar<NetItemInstance>(ownerAuth: true);
        public SyncVar<NetItemInstance> NetItemInstance => _netItemInstance;

        public DroppedItemData Data => (DroppedItemData)_entityData;

        public Vector3 Position => transform.position;

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (_netItemInstance.value == null)
            {
                Log.Error($"{nameof(_netItemInstance)} is null, did you forget to assign it?");
                return;
            }

            HandleNetItemInstanceChanged(_netItemInstance);

            _netItemInstance.onChanged += HandleNetItemInstanceChanged;
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _netItemInstance.onChanged -= HandleNetItemInstanceChanged;
        }

        public void SetNetItemInstance(NetItemInstance netItemInstance)
        {
            _netItemInstance.value = netItemInstance;
        }

        private void HandleNetItemInstanceChanged(NetItemInstance netItemInstance)
        {
            _spriteRenderer.sprite = netItemInstance.ItemId != ItemId.None ? _itemManager.GetItemData(netItemInstance.ItemId).Sprite : null;
        }

        public void Interact()
        {
            if (_context.LocalPlayer.Inventory.TryAddItem(InventoryChangeParams.Create(_netItemInstance), false, out _, out _, out _))
            {
                DespawnRpc();
            }
        }

        [ServerRpc]
        private void DespawnRpc()
        {
            _entityManager.Despawn(this);
        }
    }
}