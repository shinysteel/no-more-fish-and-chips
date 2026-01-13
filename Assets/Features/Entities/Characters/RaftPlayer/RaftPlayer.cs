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

        private Inventory _inventory;
        public Inventory Inventory => _inventory;

        private Vector2 _directionInput;

        private bool _jumpInput;
        private float _jumpTimer;

        private bool _isGrounded;
        private RaycastHit[] _groundedHitsNonAlloc = new RaycastHit[1];

        private Collider[] _swimCollidersNonAlloc = new Collider[1];

        protected override void OnSpawned()
        {
            base.OnSpawned();

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

        protected override void OnDespawned()
        {
            base.OnDespawned();

            if (!isOwner)
            {
                return;
            }
        }

        private void Update()
        {
            InputUpdate();
            MovementUpdate();
            JumpUpdate();
        }

        private void FixedUpdate()
        {
            GroundDetectionFixedUpdate();
            SwimFixedUpdate();
        }

        private void InputUpdate()
        {
            if (!isOwner)
            {
                return;
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            _directionInput = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);

            _jumpInput = Input.GetKeyDown(KeyCode.Space);
        }

        private void MovementUpdate()
        {
            if (!isOwner)
            {
                return;
            }

            Vector3 moveDirection = new Vector3(_directionInput.x, 0f, _directionInput.y);
            Vector3 targetVelocity = moveDirection * Data.MoveSettings.Speed;
            targetVelocity.y = _rigidbody.linearVelocity.y;
            float speed = moveDirection != Vector3.zero ? Data.MoveSettings.Acceleration : Data.MoveSettings.Deceleration;
            _rigidbody.linearVelocity = Vector3.MoveTowards(_rigidbody.linearVelocity, targetVelocity, speed * Time.fixedDeltaTime);
        }

        private void JumpUpdate()
        {
            if (!isOwner)
            {
                return;
            }

            _jumpTimer += Time.deltaTime;

            if (!_jumpInput)
            {
                return;
            }

            if (!_isGrounded)
            {
                return;
            }

            if (_jumpTimer < Data.JumpSettings.Cooldown)
            {
                return;
            }

            // Cancel out gravity
            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);

            _rigidbody.AddForce(Vector3.up * Data.JumpSettings.Strength, ForceMode.Impulse);

            _jumpTimer = 0f;
        }

        private void GroundDetectionFixedUpdate()
        {
            if (!isOwner)
            {
                return;
            }

            Vector3 origin = transform.position + Vector3.up * Data.GroundDetectionSettings.CastRadius;
            _isGrounded = Physics.SphereCastNonAlloc(origin, Data.GroundDetectionSettings.CastRadius, Vector3.down, _groundedHitsNonAlloc, Data.GroundDetectionSettings.CastDist, Data.GroundDetectionSettings.Mask) > 0;
        }

        private void SwimFixedUpdate()
        {
            // While swimming, the player can hold spacebar to propel themselves up
            if (!Input.GetKey(KeyCode.Space))
            {
                return;
            }

            // If we are overlapping a collider on the swim mask, we are swimming
            if (Physics.OverlapCapsuleNonAlloc(_capsuleCollider.bounds.min, _capsuleCollider.bounds.max, _capsuleCollider.radius, _swimCollidersNonAlloc, Data.SwimSettings.Mask) == 0)
            {
                return;
            }

            Collider waterCollider = _swimCollidersNonAlloc[0];

            Physics.ComputePenetration(_capsuleCollider, transform.position, transform.rotation, waterCollider, waterCollider.transform.position, waterCollider.transform.rotation, out _, out float depth);

            float ascendFactor = Mathf.Clamp01(depth / Data.SwimSettings.AscendDepthThreshold);
            Vector3 ascendForce = Vector3.up * Data.SwimSettings.AscendStrength * ascendFactor;

            _rigidbody.AddForce(ascendForce, ForceMode.Force);
        }
    }
}