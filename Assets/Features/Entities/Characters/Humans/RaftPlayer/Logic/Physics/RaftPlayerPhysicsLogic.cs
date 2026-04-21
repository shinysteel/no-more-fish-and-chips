using FishFlingers.Cameras;
using PurrNet;
using ShinyOwl.Common;
using System;
using System.Globalization;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class RaftPlayerPhysicsLogic
    {
        private CameraManager _cameraManager;

        private RaftPlayer _player;
        private CapsuleCollider _capsuleCollider;

        private RaftPlayerPhysicsSettings _settings;

        private float _jumpTimer;
        private bool _jumpRequest;
        private bool _isGrounded;

        private RaycastHit[] _groundedHitsNonAlloc = new RaycastHit[2];
        private Collider[] _swimCollidersNonAlloc = new Collider[1];

        public RaftPlayerPhysicsLogic(RaftPlayer player, CapsuleCollider capsuleCollider)
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();

            _player = player;
            _capsuleCollider = capsuleCollider;

            _settings = _player.Data.PhysicsSettings;
        }

        public void Tick()
        {
            JumpTick();
        }

        public void FixedTick()
        {
            MoveFixedTick();
            LookFixedTick();
            GroundDetectionFixedTick();
            JumpFixedTick();
            SwimFixedTick();
        }

        private void JumpTick()
        {
            _jumpTimer += Time.deltaTime;

            if (!_player.InputLogic.Jump)
            {
                return;
            }

            if (_jumpTimer < _settings.Jump.Cooldown)
            {
                return;
            }

            // Jump on the next physics step
            _jumpRequest = true;
        }

        private void MoveFixedTick()
        {
            if (_player.AttackLogic.AttackState == RaftPlayerAttackState.Impact)
            {
                return;
            }

            Vector3 targetVelocity = _player.InputLogic.MoveDirection * _settings.Move.Speed;

            if (_player.AttackLogic.AttackState == RaftPlayerAttackState.Windup)
            {
                targetVelocity *= _settings.Move.AttackWindupMultiplier;
            }
            else if (_player.AttackLogic.AttackState == RaftPlayerAttackState.Impact)
            {
                targetVelocity *= _settings.Move.AttackImpactMultiplier;
            }

            targetVelocity.y = _player.Rigidbody.linearVelocity.y;

            float speed = _player.InputLogic.MoveDirection != Vector3.zero ? _settings.Move.Acceleration : _settings.Move.Deceleration;

            _player.Rigidbody.linearVelocity = Vector3.MoveTowards(_player.Rigidbody.linearVelocity, targetVelocity, speed * Time.fixedDeltaTime);
        }

        private void LookFixedTick()
        {
            Vector3 direction = _player.InputLogic.MoveDirection;

            if (direction == Vector3.zero)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            float speed = _settings.Look.Speed;

            if (_player.AttackLogic.AttackState == RaftPlayerAttackState.Impact)
            {
                speed *= _settings.Look.AttackImpactMultiplier;
            }

            _player.Rigidbody.MoveRotation(Quaternion.Slerp(_player.Rigidbody.rotation, targetRotation, speed * Time.fixedDeltaTime));
        }

        private void GroundDetectionFixedTick()
        {
            Vector3 origin = _player.Rigidbody.position + Vector3.up * _settings.GroundDetection.CastRadius;

            int hits = Physics.SphereCastNonAlloc(origin, _settings.GroundDetection.CastRadius, Vector3.down, _groundedHitsNonAlloc, _settings.GroundDetection.CastDist, _settings.GroundDetection.Mask);

            bool grounded = false;

            for (int i = 0; i < hits; i++)
            {
                // Since we include the player layer to jump on other player's heads, we need to ignore our own collider here
                if (_groundedHitsNonAlloc[i].collider != _capsuleCollider)
                {
                    grounded = true;
                    break;
                }
            }

            _isGrounded = grounded;
        }

        private void JumpFixedTick()
        {
            if (!_jumpRequest)
            {
                return;
            }

            // Consume the request
            _jumpTimer = 0f;
            _jumpRequest = false;

            if (!_isGrounded)
            {
                return;
            }

            // Cancel out gravity
            _player.Rigidbody.linearVelocity = new Vector3(_player.Rigidbody.linearVelocity.x, 0f, _player.Rigidbody.linearVelocity.z);
            _player.Rigidbody.AddForce(Vector3.up * _settings.Jump.Strength, ForceMode.Impulse);
        }

        private void SwimFixedTick()
        {
            // While swimming, the player can hold spacebar to propel themselves up
            if (!_player.InputLogic.Ascend)
            {
                return;
            }

            // If we are overlapping a collider on the swim mask, we are swimming
            if (Physics.OverlapCapsuleNonAlloc(_capsuleCollider.bounds.min, _capsuleCollider.bounds.max, _capsuleCollider.radius, _swimCollidersNonAlloc, _settings.Swim.Mask) == 0)
            {
                return;
            }

            Collider waterCollider = _swimCollidersNonAlloc[0];

            Physics.ComputePenetration(_capsuleCollider, _player.Rigidbody.position, _player.Rigidbody.rotation, waterCollider, waterCollider.transform.position, waterCollider.transform.rotation, out _, out float depth);

            float ascendFactor = Mathf.Clamp01(depth / _settings.Swim.AscendDepthThreshold);
            Vector3 ascendForce = Vector3.up * _settings.Swim.AscendStrength * ascendFactor;

            _player.Rigidbody.AddForce(ascendForce, ForceMode.Force);
        }
    }
}