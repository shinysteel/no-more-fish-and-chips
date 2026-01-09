using FishFlingers.Cameras;
using FishFlingers.Environments;
using FishFlingers.Items;
using ShinyOwl.Common;
using ShinyOwl.Common.Structures;
using System;
using UnityEngine;
using System.Threading.Tasks;

using Random = UnityEngine.Random;

namespace FishFlingers.Entities
{
    public class RaftPlayer : NetEntity
    {
        [Serializable]
        private class MoveSettings
        {
            [SerializeField] private float _speed = 2f;
            [SerializeField] private float _acceleration = 10f;
            [SerializeField] private float _deceleration = 7.5f;

            public float Speed => _speed;
            public float Acceleration => _acceleration;
            public float Deceleration => _deceleration;
        }

        [Serializable]
        private class JumpSettings
        {
            [SerializeField] private float _strength = 4f;
            [SerializeField] private float _cooldown = 0.1f;

            public float Strength => _strength;
            public float Cooldown => _cooldown;
        }

        [Serializable]
        private class GroundDetectionSettings
        {
            [SerializeField] private LayerMask _mask;
            [SerializeField] private float _castRadius = 0.125f;
            [SerializeField] private float _castDist = 0.05f;

            public LayerMask Mask => _mask;
            public float CastRadius => _castRadius;
            public float CastDist => _castDist;
        }

        [Serializable]
        private class SwimSettings
        {
            [SerializeField] private LayerMask _mask;
            [SerializeField] private float _ascendStrength = 30f;
            [SerializeField] private float _ascendDepthThreshold = 0.25f;

            public LayerMask Mask => _mask;
            public float AscendStrength => _ascendStrength;
            public float AscendDepthThreshold => _ascendDepthThreshold;
        }

        [SerializeField] private CapsuleCollider _capsuleCollider;

        [SerializeField] private Inventory _inventoryPrefab;
        [SerializeField] private BoolGrid _inventoryGrid;

        [SerializeField] private MoveSettings _moveSettings;
        [SerializeField] private JumpSettings _jumpSettings;
        [SerializeField] private GroundDetectionSettings _groundDetectionSettings;
        [SerializeField] private SwimSettings _swimSettings;

        private Inventory _inventory;

        private Vector2 _directionInput;

        private bool _jumpInput;
        private float _jumpTimer;

        private bool _isGrounded;
        private RaycastHit[] _groundedHitsNonAlloc = new RaycastHit[1];

        private Collider[] _swimCollidersNonAlloc = new Collider[1];

        public Inventory Inventory => _inventory;

        protected override void OnSpawned()
        {
            base.OnSpawned();

            _ = OnSpawnedAsync();

            if (!isOwner)
            {
                return;
            }

            _inventory = _networkManager.Spawn(_inventoryPrefab);
            _inventory.Initialise(_inventoryGrid);

            _cameraManager.SetMode(new FollowCameraMode(transform, new Vector3(0f, 3f, -5f)));
        }

        private async Task OnSpawnedAsync()
        {
            while (!_isInitialised)
            {
                await Task.Yield();
            }

            // Spawn on a random starting tile
            transform.position = _context.Raft.TryGetRandomTile(out Tile tile) ? _context.Raft.CellToWorldPosition(tile.Cell) : Vector3.zero;
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
            Vector3 targetVelocity = moveDirection * _moveSettings.Speed;
            targetVelocity.y = _rigidbody.linearVelocity.y;
            float speed = moveDirection != Vector3.zero ? _moveSettings.Acceleration : _moveSettings.Deceleration;
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

            if (_jumpTimer < _jumpSettings.Cooldown)
            {
                return;
            }

            // Cancel out gravity
            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);

            _rigidbody.AddForce(Vector3.up * _jumpSettings.Strength, ForceMode.Impulse);

            _jumpTimer = 0f;
        }

        private void GroundDetectionFixedUpdate()
        {
            if (!isOwner)
            {
                return;
            }

            Vector3 origin = transform.position + Vector3.up * _groundDetectionSettings.CastRadius;
            _isGrounded = Physics.SphereCastNonAlloc(origin, _groundDetectionSettings.CastRadius, Vector3.down, _groundedHitsNonAlloc, _groundDetectionSettings.CastDist, _groundDetectionSettings.Mask) > 0;
        }

        private void SwimFixedUpdate()
        {
            // While swimming, the player can hold spacebar to propel themselves up
            if (!Input.GetKey(KeyCode.Space))
            {
                return;
            }

            // If we are overlapping a collider on the swim mask, we are swimming
            if (Physics.OverlapCapsuleNonAlloc(_capsuleCollider.bounds.min, _capsuleCollider.bounds.max, _capsuleCollider.radius, _swimCollidersNonAlloc, _swimSettings.Mask) == 0)
            {
                return;
            }

            Collider waterCollider = _swimCollidersNonAlloc[0];

            Physics.ComputePenetration(_capsuleCollider, transform.position, transform.rotation, waterCollider, waterCollider.transform.position, waterCollider.transform.rotation, out _, out float depth);

            float ascendFactor = Mathf.Clamp01(depth / _swimSettings.AscendDepthThreshold);
            Vector3 ascendForce = Vector3.up * _swimSettings.AscendStrength * ascendFactor;

            _rigidbody.AddForce(ascendForce, ForceMode.Force);
        }
    }
}