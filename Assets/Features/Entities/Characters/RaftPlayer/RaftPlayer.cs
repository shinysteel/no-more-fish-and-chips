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

using Random = UnityEngine.Random;

namespace FishFlingers.Entities
{
    public class RaftPlayer : Character<RaftPlayerData>
    {
        [SerializeField] private CapsuleCollider _capsuleCollider;

        [SerializeField] private Inventory _inventoryPrefab;
        [SerializeField] private BoolGrid _inventoryLayout;

        private PhysicsLogic _physicsLogic;
        private InteractLogic _interactLogic;

        private Inventory _inventory;
        public Inventory Inventory => _inventory;

        protected override void OnSpawned()
        {
            base.OnSpawned();

            _physicsLogic = new PhysicsLogic(this, _capsuleCollider);
            _interactLogic = new InteractLogic(this);

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

            _physicsLogic.Tick();
            _interactLogic.Tick();
        }

        private void FixedUpdate()
        {
            if (!isFullySpawned)
            {
                return;
            }

            _physicsLogic.FixedTick();
        }
    }
}