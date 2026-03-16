using FishFlingers.Cameras;
using FishFlingers.Environments;
using FishFlingers.Inventories;
using FishFlingers.Items;
using FishFlingers.Networking;
using FishFlingers.States;
using FishFlingers.UI;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Structures;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace FishFlingers.Entities
{
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
        private RaftPlayerGrabbedItemLogic _grabbedItemLogic;
        private RaftPlayerDropItemLogic _dropItemLogic;
        private RaftPlayerHotkeyLogic _hotkeyLogic;
        private RaftPlayerTargetLogic _targetLogic;

        public RaftPlayerInputLogic InputLogic => _inputLogic;
        public RaftPlayerInteractLogic InteractLogic => _interactLogic;
        public RaftPlayerGrabbedItemLogic GrabbedItemLogic => _grabbedItemLogic;
        public RaftPlayerDropItemLogic DropItemLogic => _dropItemLogic;
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
            _grabbedItemLogic = new RaftPlayerGrabbedItemLogic(this, _netGrabbedInventoryItem);
            _dropItemLogic = new RaftPlayerDropItemLogic(this);

            if (isOwner)
            {
                _inventory.SetLayout(_inventoryLayout);

                _ = SetupStartingItemsAsync();

                _cameraManager.SetMode(new FollowCameraMode(transform, new Vector3(0f, 3f, -5f)));
            }

            // Invoke OnNetworkSpawn after logic components are created
            base.OnSpawned();
        }

        private async Task SetupStartingItemsAsync()
        {
            while (!_inventory.NetInventoryItems.IsReady)
            {
                await Task.Yield();
            }

            // Start with a hammer and some driftwood
            _inventory.TryAddItem(new InventoryChangeParams() { ItemId = ItemId.Hammer, Count = 1 }, false, out _, out _, out _);
            _inventory.TryAddItem(new InventoryChangeParams() { ItemId = ItemId.Driftwood, Count = 15 }, false, out _, out _, out _);
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

        public async Task LoadDataAsync(string guid)
        {
            Saving.RaftPlayerSave data = await GetDataRpc(guid);
            _saveManager.LoadRaftPlayer(this, data);
        }

        [ServerRpc]
        private async Task<Saving.RaftPlayerSave> GetDataRpc(string guid)
        {
            return _saveManager.GetRaftPlayerSave(guid, _context);
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
    }
}