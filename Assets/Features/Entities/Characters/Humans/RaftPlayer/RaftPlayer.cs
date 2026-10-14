using Newtonsoft.Json;
using NoMoreFishAndChips.Cameras;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Hitboxes;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Saving;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Structures;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

using Random = UnityEngine.Random;
using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayer : Character<RaftPlayerDefinitionData>
    {
        [SerializeField] private Inventory _inventory;
        [SerializeField] private Hotbar _hotbar;

        private UIManager _uiManager;
        private NetworkManager _networkManager;

        // SyncVars
        private SyncVar<NetInventoryItem> _netGrabbedInventoryItem = new SyncVar<NetInventoryItem>(ownerAuth: true);
        private SyncVar<Vector2> _netMousePositionNormalised = new SyncVar<Vector2>(ownerAuth: true);
        private SyncVar<NetBehaviour> _netOpenNetBehaviour = new SyncVar<NetBehaviour>(ownerAuth: true);
        private SyncVar<bool> _netInBarrel = new SyncVar<bool>(ownerAuth: true);
        private SyncVar<bool> _netIsReady = new SyncVar<bool>(ownerAuth: true);

        private UsernameUI _usernameUI;

        public bool IsLocalPlayer => this == _context.LocalPlayer;

        public HumanModel HumanModel => (HumanModel)_entityModel;

        public Inventory Inventory => _inventory;
        public Hotbar Hotbar => _hotbar;

        public RaftPlayerDefeatLogic RaftPlayerDefeatLogic => (RaftPlayerDefeatLogic)EntityDefeatLogic;
        public RaftPlayerActLogic RaftPlayerActLogic => (RaftPlayerActLogic)CharacterActLogic;

        public RaftPlayerInputLogic InputLogic => GetLogic<RaftPlayerInputLogic>();
        public RaftPlayerInteractLogic InteractLogic => GetLogic<RaftPlayerInteractLogic>();
        public RaftPlayerGrabbedInventoryItemLogic GrabbedInventoryItemLogic => GetLogic<RaftPlayerGrabbedInventoryItemLogic>();
        public RaftPlayerDropInventoryItemLogic DropInventoryItemLogic => GetLogic<RaftPlayerDropInventoryItemLogic>();
        public RaftPlayerAnimateLogic AnimateLogic => GetLogic<RaftPlayerAnimateLogic>();
        public RaftPlayerOpenNetBehaviourLogic OpenNetBehaviourLogic => GetLogic<RaftPlayerOpenNetBehaviourLogic>();
        public RaftPlayerAttackLogic AttackLogic => GetLogic<RaftPlayerAttackLogic>();
        public RaftPlayerReadyLogic ReadyLogic => GetLogic<RaftPlayerReadyLogic>();
        public RaftPlayerHotkeyLogic HotkeyLogic => GetLogic<RaftPlayerHotkeyLogic>();
        public RaftPlayerBuildLogic BuildLogic => GetLogic<RaftPlayerBuildLogic>();
        public RaftPlayerContextActionsLogic ContextActionsLogic => GetLogic<RaftPlayerContextActionsLogic>();

        public class PlaceInventoryItemResponse
        {
            public bool Success { get; private set; }
            public int Overflow { get; private set; }
            public bool WasChange { get; private set; }

            public PlaceInventoryItemResponse(bool success, int overflow, bool wasChange)
            {
                Success = success;
                Overflow = overflow;
                WasChange = wasChange;
            }
        }

        public class AddInventoryItemResponse
        {
            public bool Success { get; private set; }
            public int Overflow { get; private set; }

            public AddInventoryItemResponse(bool success, int overflow)
            {
                Success = success;
                Overflow = overflow;
            }
        }

        protected override EntityLogicFactory CreateLogicFactory()
        {
            return new RaftPlayerLogicFactory(_netInBarrel);
        }

        protected override void Awake()
        {
            base.Awake();

            _uiManager = GameManager.Instance.Get<UIManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();
        }

        protected override void OnInitializeModules()
        {
            base.OnInitializeModules();

            AddLogic(typeof(RaftPlayerInputLogic), new RaftPlayerInputLogic(this, _netMousePositionNormalised));
            AddLogic(typeof(RaftPlayerInteractLogic), new RaftPlayerInteractLogic(this));
            AddLogic(typeof(RaftPlayerGrabbedInventoryItemLogic), new RaftPlayerGrabbedInventoryItemLogic(this, _netGrabbedInventoryItem));
            AddLogic(typeof(RaftPlayerDropInventoryItemLogic), new RaftPlayerDropInventoryItemLogic(this));
            AddLogic(typeof(RaftPlayerAnimateLogic), new RaftPlayerAnimateLogic(this));
            AddLogic(typeof(RaftPlayerEquippedInventoryItemsLogic), new RaftPlayerEquippedInventoryItemsLogic(this));
            AddLogic(typeof(RaftPlayerOpenNetBehaviourLogic), new RaftPlayerOpenNetBehaviourLogic(this, _netOpenNetBehaviour));
            AddLogic(typeof(RaftPlayerAttackLogic), new RaftPlayerAttackLogic(this));
            AddLogic(typeof(RaftPlayerReadyLogic), new RaftPlayerReadyLogic(this, _netIsReady));
            AddLogic(typeof(RaftPlayerHotkeyLogic), new RaftPlayerHotkeyLogic(this, _netGrabbedInventoryItem));
            AddLogic(typeof(RaftPlayerBuildLogic), new RaftPlayerBuildLogic(this));
            AddLogic(typeof(RaftPlayerContextActionsLogic), new RaftPlayerContextActionsLogic(this));
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (isOwner)
            {
                _inventory.SetLayouts(DefinitionData.UnlockableInventoryLayout, DefinitionData.DefaultUnlockedInventoryLayout);
            }
            
            if (!isOwner)
            { 
                _usernameUI = _uiManager.CreateWorldUI(_uiManager.Config.UsernameUIPrefab, Vector3.zero);
                _usernameUI.Setup(_networkManager.GetPurrnetPlayer(owner.Value));
            }
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            if (_usernameUI != null)
            {
                _uiManager.DestroyWorldUI(_usernameUI);
                _usernameUI = null;
            }
        }

        protected override void HitLocal(Hit hit, Vector3 direction)
        {
            base.HitLocal(hit, direction);

            // Manual AnimateHurt, since RaftPlayers aren't damageable but we still want to show it
            if (isSpawned)
            {
                EntityEffectsLogic.AnimateHurt();
            }
        }

        public void SetPosition(Vector3 position)
        {
            if (isOwner)
            {
                SetPositionLocal(position);
            }
            else
            {
                SetPositionRpc(owner.Value, position);
            }
        }

        [TargetRpc]
        private void SetPositionRpc(PlayerID id, Vector3 position)
        {
            SetPositionLocal(position);
        }

        private void SetPositionLocal(Vector3 position)
        {
            // Since interpolation is enabled, we need to teleport via rigidbody.position
            _rigidbody.position = new Vector3(Random.Range(-4f, 4f), 0.5f, 5f);
        }

        public void SetNetInBarrel(bool barrel)
        {
            if (isOwner)
            {
                SetNetInBarrelLocal(barrel);
            }
            else
            {
                SetNetInBarrelRpc(owner.Value, barrel);
            }
        }

        [TargetRpc]
        private void SetNetInBarrelRpc(PlayerID id, bool barrel)
        {
            SetNetInBarrelLocal(barrel);
        }

        private void SetNetInBarrelLocal(bool barrel)
        {
            _netInBarrel.value = barrel;
        }

        public async Task<AddInventoryItemResponse> AddInventoryItem(Inventory inventory, InventoryChangeParams parameters)
        {
            if (inventory.isOwner)
            {
                return await AddInventoryItemLocal(inventory, parameters);
            }
            else
            {
                return await AddInventoryItemRpc(inventory.owner.Value, inventory, parameters);
            }
        }

        [TargetRpc]
        private async Task<AddInventoryItemResponse> AddInventoryItemRpc(PlayerID playerId, Inventory inventory, InventoryChangeParams parameters)
        {
            return await AddInventoryItemLocal(inventory, parameters);
        }

        private async Task<AddInventoryItemResponse> AddInventoryItemLocal(Inventory inventory, InventoryChangeParams parameters)
        {
            bool success = inventory.TryAddItem(parameters, true, out int overflow, out _, out _);
            return new AddInventoryItemResponse(success, overflow);
        }

        public async Task<PlaceInventoryItemResponse> PlaceInventoryItem(Inventory inventory, InventoryPlaceParams placeParams)
        {
            if (inventory.isOwner)
            {
                return await PlaceInventoryItemLocal(inventory, placeParams);
            }
            else
            {
                return await PlaceInventoryItemRpc(inventory.owner.Value, inventory, placeParams);
            }
        }

        [TargetRpc]
        private async Task<PlaceInventoryItemResponse> PlaceInventoryItemRpc(PlayerID playerId, Inventory inventory, InventoryPlaceParams placeParams)
        {
            return await PlaceInventoryItemLocal(inventory, placeParams);
        }

        private async Task<PlaceInventoryItemResponse> PlaceInventoryItemLocal(Inventory inventory, InventoryPlaceParams placeParams)
        {
            bool success = inventory.TryPlaceItem(placeParams, true, out int overflow, out _, out NetInventoryItemsChange change);
            return new PlaceInventoryItemResponse(success, overflow, change.IsValid);
        }

        public async Task SetInventoryItemCount(Inventory inventory, string instanceId, int count, bool canRemove)
        {
            if (inventory.isOwner)
            {
                await SetInventoryItemCountLocal(inventory, instanceId, count, canRemove);
            }
            else
            {
                await SetInventoryItemCountRpc(inventory.owner.Value, inventory, instanceId, count, canRemove);
            }
        }

        [TargetRpc]
        private async Task SetInventoryItemCountRpc(PlayerID playerId, Inventory inventory, string instanceId, int count, bool canRemove)
        {
            await SetInventoryItemCountLocal(inventory, instanceId, count, canRemove);
        }

        private async Task SetInventoryItemCountLocal(Inventory inventory, string instanceId, int count, bool canRemove)
        {
            // No overflow indicates the item has no count left
            if (count > 0)
            {
                inventory.SetNetItemCount(instanceId, count);
                return;
            }

            if (canRemove)
            {
                inventory.TryRemoveItem(instanceId);
            }
        }

        public async Task<bool> RemoveInventoryItem(Inventory inventory, string instanceId)
        {
            if (inventory.isOwner)
            {
                return await RemoveInventoryItemLocal(inventory, instanceId);
            }
            else
            {
                return await RemoveInventoryItemRpc(inventory.owner.Value, inventory, instanceId);
            }
        }

        [TargetRpc]
        private async Task<bool> RemoveInventoryItemRpc(PlayerID playerId, Inventory inventory, string instanceId)
        {
            return await RemoveInventoryItemLocal(inventory, instanceId);
        }

        private async Task<bool> RemoveInventoryItemLocal(Inventory inventory, string instanceId)
        {
            return inventory.TryRemoveItem(instanceId);
        }

        public async Task MoveInventoryItem(Inventory fromInventory, Inventory toInventory, string instanceId)
        {
            if (fromInventory.isOwner)
            {
                await MoveInventoryItemLocal(fromInventory, toInventory, instanceId);
            }
            else
            {
                await MoveInventoryItemRpc(fromInventory.owner.Value, fromInventory, toInventory, instanceId);
            }
        }

        [TargetRpc]
        private async Task MoveInventoryItemRpc(PlayerID playerId, Inventory fromInventory, Inventory toInventory, string instanceId)
        {
            await MoveInventoryItemLocal(fromInventory, toInventory, instanceId);
        }

        private async Task MoveInventoryItemLocal(Inventory fromInventory, Inventory toInventory, string instanceId)
        {
            if (!fromInventory.InventoryItems.TryGetValue(instanceId, out InventoryItem inventoryItem))
            {
                return;
            }

            if (!inventoryItem.IsAvailable)
            {
                return;
            }

            InventoryChangeParams parameters = InventoryChangeParams.Create(inventoryItem.ItemInstance);

            if (!toInventory.CanAddItem(parameters, out _, out _, out _))
            {
                return;
            }

            fromInventory.SetNetItemIsLocked(instanceId, true);

            AddInventoryItemResponse response = await AddInventoryItemRpc(toInventory.owner.Value, toInventory, parameters);

            if (response.Success)
            {
                await SetInventoryItemCountRpc(fromInventory.owner.Value, fromInventory, instanceId, response.Overflow, true);
            }

            if (response.Overflow > 0)
            {
                fromInventory.SetNetItemIsLocked(instanceId, false);
            }
        }

        public async Task<NetInventoryItem> GrabInventoryItem(Inventory inventory, string instanceId, Vector2Int cell)
        {
            if (inventory.isOwner)
            {
                return await GrabInventoryItemLocal(inventory, instanceId, cell);
            }
            else
            {
                return await GrabInventoryItemRpc(inventory.owner.Value, inventory, instanceId, cell);
            }
        }

        [TargetRpc]
        private async Task<NetInventoryItem> GrabInventoryItemRpc(PlayerID playerId, Inventory inventory, string instanceId, Vector2Int cell)
        {
            return await GrabInventoryItemLocal(inventory, instanceId, cell);
        }

        private async Task<NetInventoryItem> GrabInventoryItemLocal(Inventory inventory, string instanceId, Vector2Int cell)
        {
            if (!inventory.InventoryItems.TryGetValue(instanceId, out InventoryItem inventoryItem))
            {
                return null;
            }

            if (!inventoryItem.IsAvailable)
            {
                return null;
            }

            // The item needs to be a clone so that rotating it doesn't affect the original
            NetInventoryItem grabbedNetInventoryItem = inventory.GetNetInventoryItemDeepClone(instanceId);

            // The slot we grabbed at becomes the pivot
            grabbedNetInventoryItem.SetPivot(InventoryItemUtils.RecalculatePivot(grabbedNetInventoryItem.Cell, cell, grabbedNetInventoryItem.Pivot, grabbedNetInventoryItem.Rotations));

            inventory.SetNetItemIsGrabbed(instanceId, true);

            return grabbedNetInventoryItem;
        }

        public async Task ReleaseInventoryItem(Inventory inventory, string instanceId)
        {
            if (inventory.isOwner)
            {
                await ReleaseInventoryItemLocal(inventory, instanceId);
            }
            else
            {
                await ReleaseInventoryItemRpc(inventory.owner.Value, inventory, instanceId);
            }
        }

        [TargetRpc]
        private async Task ReleaseInventoryItemRpc(PlayerID playerId, Inventory inventory, string instanceId)
        {
            await ReleaseInventoryItemLocal(inventory, instanceId);
        }
        
        private async Task ReleaseInventoryItemLocal(Inventory inventory, string instanceId)
        {
            // There's scenarios where you release an item, and it no longer exists in the inventory since it was moved to another
            if (inventory.InventoryItems.ContainsKey(instanceId))
            {
                inventory.SetNetItemIsGrabbed(instanceId, false);
            }
        }
    }
}