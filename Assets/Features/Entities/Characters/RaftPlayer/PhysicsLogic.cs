using FishFlingers.Cameras;
using PurrNet;
using ShinyOwl.Common;
using System.Globalization;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class PhysicsLogic
    {
        private CameraManager _cameraManager;

        private RaftPlayer _player;
        private CapsuleCollider _capsuleCollider;

        private Vector2 _directionInput;
        private float _jumpTimer;
        private bool _jumpInput;
        private bool _jumpRequest;
        private bool _isGrounded;
        private bool _ascendInput;

        private RaycastHit[] _groundedHitsNonAlloc = new RaycastHit[5];
        private Collider[] _swimCollidersNonAlloc = new Collider[1];

        public PhysicsLogic(RaftPlayer player, CapsuleCollider capsuleCollider)
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();

            _player = player;       
            _capsuleCollider = capsuleCollider;
        }

        public void Tick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            InputTick();
            JumpTick();
        }

        public void FixedTick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            MoveFixedTick();
            LookFixedTick();
            JumpFixedTick();
            GroundDetectionFixedTick();
            SwimFixedTick();
        }

        private void InputTick()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            _directionInput = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);

            _jumpInput = Input.GetKeyDown(KeyCode.Space);
            _ascendInput = Input.GetKey(KeyCode.Space);
        }

        private void JumpTick()
        {
            _jumpTimer += Time.deltaTime;

            if (!_jumpInput)
            {
                return;
            }

            if (!_isGrounded)
            {
                return;
            }

            if (_jumpTimer < _player.Data.JumpSettings.Cooldown)
            {
                return;
            }

            // Jump on the next physics step
            _jumpRequest = true;
        }

        private void MoveFixedTick()
        {
            Vector3 moveDirection = new Vector3(_directionInput.x, 0f, _directionInput.y);
            Vector3 targetVelocity = moveDirection * _player.Data.MoveSettings.Speed;
            targetVelocity.y = _player.Rigidbody.linearVelocity.y;
            float speed = moveDirection != Vector3.zero ? _player.Data.MoveSettings.Acceleration : _player.Data.MoveSettings.Deceleration;

            _player.Rigidbody.linearVelocity = Vector3.MoveTowards(_player.Rigidbody.linearVelocity, targetVelocity, speed * Time.fixedDeltaTime);
        }

        private void LookFixedTick()
        {
            Ray ray = _cameraManager.MainCamera.ScreenPointToRay(Input.mousePosition);

            // Have the plane sit at the player's origin so that y does not influence the target
            Plane plane = new Plane(Vector3.up, _player.transform.position);

            // Face the cursor
            if (!plane.Raycast(ray, out float distance))
            {
                return;
            }

            Vector3 direction = (ray.GetPoint(distance) - _player.transform.position).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            _player.Rigidbody.MoveRotation(Quaternion.Slerp(_player.Rigidbody.rotation, targetRotation, _player.Data.LookSettings.Speed * Time.fixedDeltaTime));
        }

        private void JumpFixedTick()
        {
            if (!_jumpRequest)
            {
                return;
            }

            // Cancel out gravity
            _player.Rigidbody.linearVelocity = new Vector3(_player.Rigidbody.linearVelocity.x, 0f, _player.Rigidbody.linearVelocity.z);
            _player.Rigidbody.AddForce(Vector3.up * _player.Data.JumpSettings.Strength, ForceMode.Impulse);

            // Consume the request
            _jumpTimer = 0f;
            _jumpRequest = false;
        }

        private void GroundDetectionFixedTick()
        {
            Vector3 origin = _player.Rigidbody.position + Vector3.up * _player.Data.GroundDetectionSettings.CastRadius;
            _isGrounded = Physics.SphereCastNonAlloc(origin, _player.Data.GroundDetectionSettings.CastRadius, Vector3.down, _groundedHitsNonAlloc, _player.Data.GroundDetectionSettings.CastDist, _player.Data.GroundDetectionSettings.Mask) > 0;
        }

        private void SwimFixedTick()
        {
            // While swimming, the player can hold spacebar to propel themselves up
            if (!_ascendInput)
            {
                return;
            }

            // If we are overlapping a collider on the swim mask, we are swimming
            if (Physics.OverlapCapsuleNonAlloc(_capsuleCollider.bounds.min, _capsuleCollider.bounds.max, _capsuleCollider.radius, _swimCollidersNonAlloc, _player.Data.SwimSettings.Mask) == 0)
            {
                return;
            }

            Collider waterCollider = _swimCollidersNonAlloc[0];

            Physics.ComputePenetration(_capsuleCollider, _player.Rigidbody.position, _player.Rigidbody.rotation, waterCollider, waterCollider.transform.position, waterCollider.transform.rotation, out _, out float depth);

            float ascendFactor = Mathf.Clamp01(depth / _player.Data.SwimSettings.AscendDepthThreshold);
            Vector3 ascendForce = Vector3.up * _player.Data.SwimSettings.AscendStrength * ascendFactor;

            _player.Rigidbody.AddForce(ascendForce, ForceMode.Force);
        }
    }
}