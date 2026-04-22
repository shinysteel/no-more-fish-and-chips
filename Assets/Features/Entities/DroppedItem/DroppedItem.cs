using PurrNet;
using UnityEngine;
using FishFlingers.Environments;
using ShinyOwl.Common;
using FishFlingers.Items;
using FishFlingers.Inventories;
using Newtonsoft.Json;
using ShinyOwl.Common.Utils;
using NUnit.Framework;
using System.Collections.Generic;

namespace FishFlingers.Entities
{
    public enum DroppedItemType
    {
        Default,
        Salvage,
    }

    public class DroppedItem : NetEntity, IInteractable
    {
        private List<ItemModel> _itemModels = new();

        private SyncVar<NetItemInstance> _netItemInstance = new SyncVar<NetItemInstance>(ownerAuth: true);
        public SyncVar<NetItemInstance> NetItemInstance => _netItemInstance;

        private DroppedItemType _type;
        public DroppedItemType Type => _type;

        public DroppedItemData Data => (DroppedItemData)_entityData;

        public Vector3 Position => transform.position;

        private const float DespawnDistance = 15f;

        private const int MaxItemModels = 3;

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

            HandleNetItemInstanceChanged(null);
        }

        public void Set(NetItemInstance netItemInstance, DroppedItemType type)
        {
            _netItemInstance.value = netItemInstance;
            _type = type;
        }

        private void HandleNetItemInstanceChanged(NetItemInstance netItemInstance)
        {
            int count = (netItemInstance?.ItemId ?? ItemId.None) != ItemId.None ? Mathf.Min(netItemInstance.Count, MaxItemModels) : 0;

            Utils.Collections.ResizeList(_itemModels, count,
                createElement: () => _poolManager.GetItemModel(netItemInstance.ItemId, new SpawnParams() { Parent = transform }),
                removeElement: (ItemModel model) => _poolManager.ReturnItemModel(model),
                processElement: (ItemModel model, int index) => model.transform.localPosition = Data.DropOrientations[count - 1].Positions[index]);
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