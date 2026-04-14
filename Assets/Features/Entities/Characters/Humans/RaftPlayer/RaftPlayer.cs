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
using UnityEngine.UIElements;

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
        [JsonProperty] public HotbarSave Hotbar { get; private set; } = new();

        private const int Precision = 1;

        public RaftPlayerSave()
        { }

        public async Task LoadToAsync(RaftPlayer player)
        {
            player.transform.position = Position;
            player.transform.rotation = Rotation;

            player.Rigidbody.linearVelocity = Vector3.zero;
            player.Rigidbody.angularVelocity = Vector3.zero;

            await Inventory.LoadToAsync(player.Inventory);

            Hotbar.LoadTo(player.Hotbar);
        }

        public void SaveFrom(RaftPlayer player)
        {
            Position = Utils.Math.RoundVector3(player.transform.position, Precision);
            Rotation = Utils.Math.RoundQuaternion(player.transform.rotation, Precision);

            Inventory.SaveFrom(player.Inventory);

            Hotbar.SaveFrom(player.Hotbar);
        }

        public void ApplyDefaults()
        {
            Inventory.Items.Add(new InventoryItemSave(Vector2Int.zero, Vector2Int.zero, 0, null, ItemId.Hammer, 1));
            Inventory.Items.Add(new InventoryItemSave(new Vector2Int(1, 0), Vector2Int.zero, 0, null, ItemId.Paddle, 1));
        }
    }

    public class RaftPlayer : Character<RaftPlayerData>
    {
        [SerializeField] private CapsuleCollider _capsuleCollider;

        [SerializeField] private Inventory _inventory;
        [SerializeField] private BoolGrid _inventoryLayout;
        [SerializeField] private Hotbar _hotbar;

        public Inventory Inventory => _inventory;
        public Hotbar Hotbar => _hotbar;

        [SerializeField] private RaftPlayerTileTargetLogicSettings _targetLogicSettings;

        private RaftPlayerInputLogic _inputLogic;
        private RaftPlayerPhysicsLogic _physicsLogic;
        private RaftPlayerInteractLogic _interactLogic;
        private RaftPlayerGrabbedInventoryItemLogic _grabbedInventoryItemLogic;
        private RaftPlayerDropInventoryItemLogic _dropInventoryItemLogic;
        private RaftPlayerAnimateLogic _animateLogic;
        private RaftPlayerHeldInventoryItemLogic _heldInventoryItemLogic;
        private RaftPlayerOpenNetBehaviourLogic _openNetBehaviourLogic;
        private RaftPlayerHotkeyLogic _hotkeyLogic;
        private RaftPlayerTileTargetLogic _tileTargetLogic;

        public RaftPlayerInputLogic InputLogic => _inputLogic;
        public RaftPlayerInteractLogic InteractLogic => _interactLogic;
        public RaftPlayerGrabbedInventoryItemLogic GrabbedInventoryItemLogic => _grabbedInventoryItemLogic;
        public RaftPlayerDropInventoryItemLogic DropInventoryItemLogic => _dropInventoryItemLogic;
        public RaftPlayerOpenNetBehaviourLogic OpenNetBehaviourLogic => _openNetBehaviourLogic;
        public RaftPlayerTileTargetLogic TileTargetLogic => _tileTargetLogic;

        public bool CanAct => !_uiManager.IsLayerInUse(UILayer.Panels);

        // SyncVars
        private SyncVar<NetInventoryItem> _netGrabbedInventoryItem = new SyncVar<NetInventoryItem>(ownerAuth: true);
        private SyncVar<Vector2> _netMousePositionNormalised = new SyncVar<Vector2>(ownerAuth: true);
        private SyncVar<NetBehaviour> _netOpenNetworkId = new SyncVar<NetBehaviour>(ownerAuth: true);

        public Vector2 MousePositionNormalised => _netMousePositionNormalised.value;

        public bool IsLocalPlayer => this == _context.LocalPlayer;

        protected override void OnSpawned()
        {
            _inputLogic = new RaftPlayerInputLogic(this);
            _physicsLogic = new RaftPlayerPhysicsLogic(this, _capsuleCollider);
            _interactLogic = new RaftPlayerInteractLogic(this);
            _grabbedInventoryItemLogic = new RaftPlayerGrabbedInventoryItemLogic(this, _netGrabbedInventoryItem);
            _dropInventoryItemLogic = new RaftPlayerDropInventoryItemLogic(this);
            _animateLogic = new RaftPlayerAnimateLogic(this, _characterModel);
            _heldInventoryItemLogic = new RaftPlayerHeldInventoryItemLogic(this, _characterModel);
            _openNetBehaviourLogic = new RaftPlayerOpenNetBehaviourLogic(_netOpenNetworkId);

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
                _hotkeyLogic = new RaftPlayerHotkeyLogic(context, _netGrabbedInventoryItem);
                _tileTargetLogic = new RaftPlayerTileTargetLogic(context, _targetLogicSettings);
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
            _animateLogic.Tick();
            _hotkeyLogic.Tick();
            _tileTargetLogic.Tick();

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

        public void SetNetOpenObjectNetworkId(NetBehaviour behaviour)
        {
            if (!isOwner)
            {
                return;
            }

            _netOpenNetworkId.value = behaviour;
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

            inventory.SetNetItemIsGrabbed(instanceId, true);

            return grabbedNetInventoryItem;
        }

        [TargetRpc]
        public async Task<RaftPlayerGrabbedInventoryItemLogic.PlaceResponse> PlaceRpc(PlayerID playerId, Inventory inventory, InventoryPlaceParams placeParams)
        {
            bool success = inventory.TryPlaceItem(placeParams, true, out int overflow, out _, out NetInventoryItemsChange change);
            return new RaftPlayerGrabbedInventoryItemLogic.PlaceResponse(success, overflow, change.IsValid);
        }

        [TargetRpc]
        public async Task SetRpc(PlayerID playerId, Inventory inventory, string instanceId, int count, bool canRemove)
        {
            // No overflow indicates the item has no count left
            if (count > 0)
            {
                inventory.SetNetItemCount(instanceId, count);
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
                inventory.SetNetItemIsGrabbed(instanceId, false);
            }
        }
    }
}