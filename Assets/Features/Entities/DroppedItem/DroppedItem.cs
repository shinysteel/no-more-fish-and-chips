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
    public enum DroppedItemType
    {
        Default,
        Salvage,
    }

    public class DroppedItemSave
    {
        [JsonProperty] public DroppedItemType Type { get; private set; }
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
            Type = droppedItem.Type;
            InstanceId = droppedItem.NetItemInstance.value.InstanceId;
            ItemId = droppedItem.NetItemInstance.value.ItemId;
            Count = droppedItem.NetItemInstance.value.Count;
            Position = Utils.Math.RoundVector3(droppedItem.transform.position, Precision);
        }
    }

    public class DroppedItem : NetEntity, IInteractable
    {
        private ItemModel _itemModel;

        private SyncVar<NetItemInstance> _netItemInstance = new SyncVar<NetItemInstance>(ownerAuth: true);
        public SyncVar<NetItemInstance> NetItemInstance => _netItemInstance;

        private DroppedItemType _type;
        public DroppedItemType Type => _type;

        public DroppedItemData Data => (DroppedItemData)_entityData;

        public Vector3 Position => transform.position;

        private const float DespawnDistance = 15f;

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

            HandleItemIdChanged(ItemId.None);
        }

        public void Set(NetItemInstance netItemInstance, DroppedItemType type)
        {
            _netItemInstance.value = netItemInstance;
            _type = type;
        }

        private void HandleNetItemInstanceChanged(NetItemInstance netItemInstance)
        {
            HandleItemIdChanged(netItemInstance.ItemId);
        }

        private void HandleItemIdChanged(ItemId itemId)
        {
            if (_itemModel != null && _itemModel.ItemId != itemId)
            {
                _poolManager.ReturnItemModel(_itemModel);
                _itemModel = null;
            }

            if (itemId != ItemId.None)
            {
                _itemModel = _poolManager.GetItemModel(itemId, new SpawnParams() { Parent = transform });
            }
        }
        
        private void Update()
        {
            // It seems calling Rpcs is unsafe before isFullySpawned is true, given there's errors if the despawn condition is immediately true on spawn
            if (isOwner && isFullySpawned)
            {
                DespawnUpdate();
            }
        }

        // Despawns when too far away from the raft
        private void DespawnUpdate()
        {
            if (Vector3.Distance(transform.position, Vector3.zero) < DespawnDistance)
            {
                return;
            }

            DespawnRpc();
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