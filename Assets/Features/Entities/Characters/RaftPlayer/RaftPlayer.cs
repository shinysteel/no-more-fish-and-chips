using FishFlingers.Cameras;
using FishFlingers.Environments;
using FishFlingers.Inventories;
using FishFlingers.Items;
using FishFlingers.Networking;
using FishFlingers.Saving;
using FishFlingers.States;
using FishFlingers.UI;
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

namespace FishFlingers.Entities
{
    public class RaftPlayerSave
    {
        [JsonProperty] private SimpleVector3 _position = new();
        [JsonProperty] private SimpleQuaternion _rotation = new();

        [JsonIgnore] public Vector3 Position
        {
            get => _position.ToVector3();
            set => _position = new SimpleVector3(value);
        }

        [JsonIgnore] public Quaternion Rotation
        {
            get => _rotation.ToQuaternion();
            set => _rotation = new SimpleQuaternion(value);
        }

        [JsonProperty] public InventorySave Inventory { get; private set; } = new();

        private const int Precision = 1;

        public RaftPlayerSave()
        { }
        
        private RaftPlayerSave(Vector3 position, Quaternion rotation, Inventory inventory)
        {
            Position = Utils.Math.RoundVector3(position, Precision);
            Rotation = Utils.Math.RoundQuaternion(rotation, Precision);

            if (inventory != null)
            {
                foreach (InventoryItem item in inventory.InventoryItems.Values)
                {
                    Inventory.Items.Add(new InventoryItemSave(item));
                }
            }
        }

        public RaftPlayerSave(RaftPlayer player) : this(player.transform.position, player.transform.rotation, player.Inventory)
        { }

        public void ApplyDefaults()
        {
            Inventory.Items.Add(new InventoryItemSave(Vector2Int.zero, Vector2Int.zero, 0, null, ItemId.Hammer, 1));
        }
    }

    public class RaftPlayer : Character<RaftPlayerData>
    {
        [SerializeField] private CapsuleCollider _capsuleCollider;

        [SerializeField] private Inventory _inventory;
        [SerializeField] private BoolGrid _inventoryLayout;
        public Inventory Inventory => _inventory;

        [SerializeField] private RaftPlayerTargetLogicSettings _targetLogicSettings;

        private Hotbar _hotbar;
        public Hotbar Hotbar => _hotbar;

        private RaftPlayerInputLogic _inputLogic;
        private RaftPlayerPhysicsLogic _physicsLogic;
        private RaftPlayerInteractLogic _interactLogic;
        private RaftPlayerGrabbedInventoryItemLogic _grabbedInventoryItemLogic;
        private RaftPlayerDropInventoryItemLogic _dropInventoryItemLogic;
        private RaftPlayerHotkeyLogic _hotkeyLogic;
        private RaftPlayerTargetLogic _targetLogic;

        public RaftPlayerInputLogic InputLogic => _inputLogic;
        public RaftPlayerInteractLogic InteractLogic => _interactLogic;
        public RaftPlayerGrabbedInventoryItemLogic GrabbedInventoryItemLogic => _grabbedInventoryItemLogic;
        public RaftPlayerDropInventoryItemLogic DropInventoryItemLogic => _dropInventoryItemLogic;
        public RaftPlayerTargetLogic TargetLogic => _targetLogic;

        public bool CanAct => !_uiManager.IsLayerInUse(UILayer.Panels);

        // SyncVars
        private SyncVar<NetInventoryItem> _netGrabbedInventoryItem = new SyncVar<NetInventoryItem>(ownerAuth: true);
        private SyncVar<Vector2> _netMousePositionNormalised = new SyncVar<Vector2>(ownerAuth: true);

        public Vector2 MousePositionNormalised => _netMousePositionNormalised.value;

        public bool IsLocalPlayer => this == _context.LocalPlayer;

        protected override void OnSpawned()
        {
            _inputLogic = new RaftPlayerInputLogic(this);
            _physicsLogic = new RaftPlayerPhysicsLogic(this, _capsuleCollider);
            _interactLogic = new RaftPlayerInteractLogic(this);
            _grabbedInventoryItemLogic = new RaftPlayerGrabbedInventoryItemLogic(this, _netGrabbedInventoryItem);
            _dropInventoryItemLogic = new RaftPlayerDropInventoryItemLogic(this);

            if (isOwner)
            {
                _inventory.SetLayout(_inventoryLayout);

                _cameraManager.SetMode(new FollowCameraMode(transform, new Vector3(0f, 3f, -5f)));
            }

            // Invoke OnNetworkSpawn after logic components are created
            base.OnSpawned();
        }

        public override void Initialise(GameplayContext context)
        {
            base.Initialise(context);

            if (isOwner)
            {
                _hotbar = new Hotbar(context);

                _hotkeyLogic = new RaftPlayerHotkeyLogic(context, _netGrabbedInventoryItem);
                _targetLogic = new RaftPlayerTargetLogic(context, _targetLogicSettings);
            }
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _interactLogic.Cleanup();
        }

        private void Update()
        {
            if (!isFullySpawned)
            {
                return;
            }

            if (!isOwner)
            {
                return;
            }

            _inputLogic.Tick();
            _physicsLogic.Tick();
            _interactLogic.Tick();
            _hotkeyLogic.Tick();
            _targetLogic.Tick();

            SyncVarsUpdate();
        }

        private void FixedUpdate()
        {
            if (!isFullySpawned)
            {
                return;
            }

            if (!isOwner)
            {
                return;
            }

            _physicsLogic.FixedTick();
        }

        private void SyncVarsUpdate()
        {   
            _netMousePositionNormalised.value = new Vector2(Mathf.Clamp01(_inputLogic.Mouse.x / Screen.width), Mathf.Clamp01(_inputLogic.Mouse.y / Screen.height));
        }

        public async Task LoadAsync(RaftPlayerSave save)
        {
            transform.position = save.Position;
            transform.rotation = save.Rotation;

            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            while (!_inventory.IsReady)
            {
                await Task.Yield();
            }

            foreach (InventoryItemSave itemSave in save.Inventory.Items)
            {
                bool place = _inventory.TryPlaceItem(InventoryPlaceParams.Create(itemSave), false, out _, out _, out _);
            }
        }

        [TargetRpc]
        public async Task<NetInventoryItem> GrabRpc(PlayerID playerId, Inventory inventory, string instanceId, Vector2Int cell)
        {
            if (!inventory.InventoryItems.TryGetValue(instanceId, out InventoryItem inventoryItem))
            {
                return null;
            }

            if (inventoryItem.IsGrabbed)
            {
                return null;
            }

            // The item needs to be a clone so that rotating it doesn't affect the original
            NetInventoryItem grabbedNetInventoryItem = inventory.GetNetInventoryItemDeepClone(instanceId);

            // The slot we grabbed at becomes the pivot
            grabbedNetInventoryItem.SetPivot(InventoryItemUtils.RecalculatePivot(grabbedNetInventoryItem.Cell, cell, grabbedNetInventoryItem.Pivot, grabbedNetInventoryItem.Rotations));

            inventory.SetItemIsGrabbed(instanceId, true);

            return grabbedNetInventoryItem;
        }

        [TargetRpc]
        public async Task<int?> PlaceRpc(PlayerID playerId, Inventory inventory, InventoryPlaceParams placeParams)
        {
            return inventory.TryPlaceItem(placeParams, true, out int overflow, out _, out _) ? overflow : null;
        }

        [TargetRpc]
        public async Task SetRpc(PlayerID playerId, Inventory inventory, string instanceId, int count, bool canRemove)
        {
            // No overflow indicates the item has no count left
            if (count > 0)
            {
                inventory.SetItemCount(instanceId, count);
                return;
            }

            if (canRemove)
            {
                inventory.RemoveItem(instanceId);
            }
        }

        [TargetRpc]
        public async Task DropRpc(PlayerID playerId, Inventory inventory, string instanceId)
        {
            inventory.RemoveItem(instanceId);
        }

        [TargetRpc]
        public async Task ReleaseRpc(PlayerID playerId, Inventory inventory, string instanceId)
        {
            // There's scenarios where you release an item, and it no longer exists in the inventory since it was moved to another
            if (inventory.InventoryItems.ContainsKey(instanceId))
            {
                inventory.SetItemIsGrabbed(instanceId, false);
            }
        }
    }
}