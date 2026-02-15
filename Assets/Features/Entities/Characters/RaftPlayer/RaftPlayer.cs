using FishFlingers.Cameras;
using FishFlingers.Environments;
using FishFlingers.Items;
using FishFlingers.Inventories;
using ShinyOwl.Common;
using ShinyOwl.Common.Structures;
using System;
using UnityEngine;
using System.Threading.Tasks;
using PurrNet;
using FishFlingers.States;
using FishFlingers.UI;

namespace FishFlingers.Entities
{
    public class RaftPlayer : Character<RaftPlayerData>
    {
        [SerializeField] private CapsuleCollider _capsuleCollider;

        [SerializeField] private Inventory _inventory;

        [SerializeField] private BoolGrid _inventoryLayout;

        private InputLogic _inputLogic;
        private PhysicsLogic _physicsLogic;
        private InteractLogic _interactLogic;
        private HeldItemLogic _heldItemLogic;

        public HeldItemLogic HeldItemLogic => _heldItemLogic;

        public Inventory Inventory => _inventory;
        public bool CanAct => _uiManager.IsLayerEmpty(UILayer.Panels);

        // SyncVars
        private SyncVar<NetInventoryItem> _netHeldInventoryItem = new(ownerAuth: true);
        private SyncVar<Vector2> _netMousePositionNormalised = new(ownerAuth: true);

        public NetInventoryItem NetHeldInventoryItem => _netHeldInventoryItem.value;
        public Vector2 MousePositionNormalised => _netMousePositionNormalised.value;

        public bool IsLocalPlayer => this == _context.LocalPlayer;

        protected override void OnSpawned()
        {
            _inputLogic = new InputLogic(this);
            _physicsLogic = new PhysicsLogic(this, _inputLogic, _capsuleCollider);
            _interactLogic = new InteractLogic(this, _inputLogic);
            _heldItemLogic = new HeldItemLogic(_netHeldInventoryItem, _inputLogic);

            if (isOwner)
            {
                _inventory.SetLayout(_inventoryLayout);

                _cameraManager.SetMode(new FollowCameraMode(transform, new Vector3(0f, 3f, -5f)));
            }

            // Invoke OnNetworkSpawn after logic components are created
            base.OnSpawned();
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _heldItemLogic.Dispose();
        }

        public override void Initialise(GameplayContext context)
        {
            base.Initialise(context);

            // Spawn on a random starting tile
            transform.position = _context.Raft.TryGetRandomTile(out RaftTile tile) ? _context.Raft.CellToWorldPosition(tile.Cell) : Vector3.zero;
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
            _heldItemLogic.Tick();

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
            _netMousePositionNormalised.value = new Vector2(Mathf.Clamp01(_inputLogic.RawMouse.x / Screen.width), Mathf.Clamp01(_inputLogic.RawMouse.y / Screen.height));
        }
    }
}