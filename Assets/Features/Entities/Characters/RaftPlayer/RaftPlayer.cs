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

        [SerializeField] private Inventory _inventoryPrefab;
        [SerializeField] private BoolGrid _inventoryLayout;

        private InputLogic _inputLogic;
        private PhysicsLogic _physicsLogic;
        private InteractLogic _interactLogic;

        private Inventory _inventory;
        public Inventory Inventory => _inventory;

        public bool CanAct => _uiManager.IsLayerEmpty(UILayer.Panels);

        protected override void OnSpawned()
        {
            base.OnSpawned();

            _inputLogic = new InputLogic(this);
            _physicsLogic = new PhysicsLogic(this, _inputLogic, _capsuleCollider);
            _interactLogic = new InteractLogic(this, _inputLogic);

            if (!isOwner)
            {
                return;
            }

            _inventory = _networkManager.Spawn(_inventoryPrefab);
            _inventory.Initialise(_inventoryLayout);

            _cameraManager.SetMode(new FollowCameraMode(transform, new Vector3(0f, 3f, -5f)));
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
    }
}