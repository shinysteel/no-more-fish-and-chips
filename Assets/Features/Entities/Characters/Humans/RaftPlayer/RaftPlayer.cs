using NoMoreFishAndChips.Cameras;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Saving;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.UI;
using Newtonsoft.Json;
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

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayer : Character<RaftPlayerDefinitionData>
    {
        [SerializeField] private Inventory _inventory;
        [SerializeField] private Hotbar _hotbar;

        public HumanModel HumanModel => (HumanModel)_entityModel;

        public Inventory Inventory => _inventory;
        public Hotbar Hotbar => _hotbar;

        private UsernameUI _usernameUI;

        private RaftPlayerInputLogic _inputLogic;
        private RaftPlayerInteractLogic _interactLogic;
        private RaftPlayerGrabbedInventoryItemLogic _grabbedInventoryItemLogic;
        private RaftPlayerDropInventoryItemLogic _dropInventoryItemLogic;
        private RaftPlayerAnimateLogic _animateLogic;
        private RaftPlayerEquippedInventoryItemsLogic _equippedInventoryItemsLogic;
        private RaftPlayerOpenNetBehaviourLogic _openNetBehaviourLogic;
        private RaftPlayerAttackLogic _attackLogic;
        private RaftPlayerHotkeyLogic _hotkeyLogic;
        private RaftPlayerTileTargetLogic _tileTargetLogic;

        public RaftPlayerDefeatModule RaftPlayerDefeatModule => (RaftPlayerDefeatModule)_entityDefeatModule;
        public RaftPlayerActLogic RaftPlayerActLogic => (RaftPlayerActLogic)_characterActLogic;

        public RaftPlayerInputLogic InputLogic => _inputLogic;
        public RaftPlayerInteractLogic InteractLogic => _interactLogic;
        public RaftPlayerGrabbedInventoryItemLogic GrabbedInventoryItemLogic => _grabbedInventoryItemLogic;
        public RaftPlayerDropInventoryItemLogic DropInventoryItemLogic => _dropInventoryItemLogic;
        public RaftPlayerAnimateLogic AnimateLogic => _animateLogic;
        public RaftPlayerOpenNetBehaviourLogic OpenNetBehaviourLogic => _openNetBehaviourLogic;
        public RaftPlayerAttackLogic AttackLogic => _attackLogic;
        public RaftPlayerTileTargetLogic TileTargetLogic => _tileTargetLogic;

        // SyncVars
        private SyncVar<NetInventoryItem> _netGrabbedInventoryItem = new SyncVar<NetInventoryItem>(ownerAuth: true);
        private SyncVar<Vector2> _netMousePositionNormalised = new SyncVar<Vector2>(ownerAuth: true);
        private SyncVar<NetBehaviour> _netOpenNetworkId = new SyncVar<NetBehaviour>(ownerAuth: true);
        private SyncVar<bool> _netInBarrel = new SyncVar<bool>(ownerAuth: true);

        public Vector2 MousePositionNormalised => _netMousePositionNormalised.value;

        public bool IsLocalPlayer => this == _context.LocalPlayer;

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

        protected override EntityDefeatModule CreateDefeatModule()
        {
            return new RaftPlayerDefeatModule(this, GetNetIsDefeated, SetNetIsDefeated, _netInBarrel);
        }

        protected override EntityPhysicsModule CreatePhysicsModule()
        {
            return new RaftPlayerPhysicsModule(this, _rigidbody, (CapsuleCollider)_collider);
        }

        protected override CharacterActLogic CreateActLogic()
        {
            return new RaftPlayerActLogic(this);
        }

        protected override void OnSpawned()
        {
            _inputLogic = new RaftPlayerInputLogic(this);
            _interactLogic = new RaftPlayerInteractLogic(this);
            _grabbedInventoryItemLogic = new RaftPlayerGrabbedInventoryItemLogic(this, _netGrabbedInventoryItem);
            _dropInventoryItemLogic = new RaftPlayerDropInventoryItemLogic(this);
            _animateLogic = new RaftPlayerAnimateLogic(this);
            _equippedInventoryItemsLogic = new RaftPlayerEquippedInventoryItemsLogic(this);
            _openNetBehaviourLogic = new RaftPlayerOpenNetBehaviourLogic(_netOpenNetworkId);
            _attackLogic = new RaftPlayerAttackLogic(this);

            if (isOwner)
            {
                _inventory.SetLayouts(DefinitionData.UnlockableInventoryLayout, DefinitionData.DefaultUnlockedInventoryLayout);
            }
            else
            {
                _usernameUI = _uiManager.CreateWorldUI(_uiManager.Config.UsernameUIPrefab, Vector3.zero);
                _usernameUI.Setup(_networkManager.GetPurrnetPlayer(owner.Value));
            }

            // Invoke OnNetworkSpawn after logic components are created
            base.OnSpawned();
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            _hotkeyLogic = new RaftPlayerHotkeyLogic(this, context, _netGrabbedInventoryItem);
            _tileTargetLogic = new RaftPlayerTileTargetLogic(this, context);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            if (_usernameUI != null)
            {
                _uiManager.DestroyWorldUI(_usernameUI);
                _usernameUI = null;
            }

            _interactLogic.Cleanup();
        }

        protected override void Update()
        {
            base.Update();
            
            if (!isFullySpawned || !_isContextInitialised)
            {
                return;
            }

            _inputLogic.Tick();
            _interactLogic.Tick();
            _animateLogic.Tick();
            _hotkeyLogic.Tick();
            _tileTargetLogic.Tick();
            _attackLogic.Tick();

            if (isOwner)
            {
                SyncVarsUpdate();
            }
        }

        private void SyncVarsUpdate()
        {   
            _netMousePositionNormalised.value = new Vector2(Mathf.Clamp01(_inputLogic.Mouse.x / Screen.width), Mathf.Clamp01(_inputLogic.Mouse.y / Screen.height));
        }

        public void SetNetOpenObjectNetworkId(NetBehaviour behaviour)
        {
            if (!isOwner)
            {
                return;
            }

            _netOpenNetworkId.value = behaviour;
        }

        [TargetRpc]
        public void SetPositionRpc(PlayerID id, Vector3 position)
        {
            // Since interpolation is enabled, we need to teleport via rigidbody.position
            _rigidbody.position = new Vector3(Random.Range(-4f, 4f), 0.5f, 5f);
        }

        [TargetRpc]
        public void SetNetInBarrelRpc(PlayerID id, bool barrel)
        {
            _netInBarrel.value = barrel;
        }

        [TargetRpc]
        public async Task<AddInventoryItemResponse> AddInventoryItemRpc(PlayerID playerId, Inventory inventory, InventoryChangeParams parameters)
        {
            bool success = inventory.TryAddItem(parameters, true, out int overflow, out _, out _);
            return new AddInventoryItemResponse(success, overflow);
        }

        [TargetRpc]
        public async Task<PlaceInventoryItemResponse> PlaceInventoryItemRpc(PlayerID playerId, Inventory inventory, InventoryPlaceParams placeParams)
        {
            bool success = inventory.TryPlaceItem(placeParams, true, out int overflow, out _, out NetInventoryItemsChange change);
            return new PlaceInventoryItemResponse(success, overflow, change.IsValid);
        }

        [TargetRpc]
        public async Task SetInventoryItemCountRpc(PlayerID playerId, Inventory inventory, string instanceId, int count, bool canRemove)
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

        [TargetRpc]
        public async Task<bool> RemoveInventoryItemRpc(PlayerID playerId, Inventory inventory, string instanceId)
        {
            return inventory.TryRemoveItem(instanceId);
        }

        [TargetRpc]
        public async Task MoveInventoryItemRpc(PlayerID playerId, Inventory fromInventory, Inventory toInventory, string instanceId)
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

        [TargetRpc]
        public async Task<NetInventoryItem> GrabInventoryItemRpc(PlayerID playerId, Inventory inventory, string instanceId, Vector2Int cell)
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

        [TargetRpc]
        public async Task ReleaseInventoryItemRpc(PlayerID playerId, Inventory inventory, string instanceId)
        {
            // There's scenarios where you release an item, and it no longer exists in the inventory since it was moved to another
            if (inventory.InventoryItems.ContainsKey(instanceId))
            {
                inventory.SetNetItemIsGrabbed(instanceId, false);
            }
        }
    }
}