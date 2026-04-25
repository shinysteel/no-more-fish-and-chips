using FishFlingers.Cameras;
using PurrNet;
using ShinyOwl.Common;
using System;
using System.Globalization;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class RaftPlayerPhysicsLogic : CharacterPhysicsLogic
    {
        private CameraManager _cameraManager;

        private RaftPlayer _player;

        private RaftPlayerPhysicsSettings _settings;

        private float _jumpTimer;
        private bool _jumpRequest;
        
        public RaftPlayerPhysicsLogic(RaftPlayer player) : base(player)
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();

            _player = player;

            _settings = _player.Data.RaftPlayerPhysicsSettings;
        }

        public override void Tick()
        {
            JumpTick();
        }

        public override void FixedTick()
        {
            base.FixedTick();

            MoveFixedTick();
            LookFixedTick();
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
            Vector3 direction;

            if (_player.Hotbar.SelectedSlot.InventoryItem?.ItemInstance.Data.ShowsTileTarget ?? false)
            {
                Ray ray = _cameraManager.MainCamera.ScreenPointToRay(_player.InputLogic.GameplayMouse);

                // Have the plane sit at the player's origin so that y does not influence the target
                Plane plane = new Plane(Vector3.up, _player.transform.position);

                // Face the cursor
                if (!plane.Raycast(ray, out float distance))
                {
                    return;
                }

                direction = (ray.GetPoint(distance) - _player.transform.position).normalized;
            }
            else
            {
                direction = _player.InputLogic.MoveDirection;
            }

            if (direction == Vector3.zero)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            float speed = _settings.Look.Speed;

            if (_player.AttackLogic.AttackState == RaftPlayerAttackState.Impact)
            {
                speed *= _settings.Look.AttackImpactMultiplier;
            }

            _player.Rigidbody.MoveRotation(Quaternion.Slerp(_player.Rigidbody.rotation, targetRotation, speed * Time.fixedDeltaTime));
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
            // While in water, the player can hold spacebar to propel themselves up
            if (!_player.InputLogic.Ascend)
            {
                return;
            }

            if (!_inWater)
            {
                return;
            }

            Collider waterCollider = _inWaterCollidersNonAlloc[0];

            Physics.ComputePenetration(_player.CapsuleCollider, _player.Rigidbody.position, _player.Rigidbody.rotation, waterCollider, waterCollider.transform.position, waterCollider.transform.rotation, out _, out float depth);

            float ascendFactor = Mathf.Clamp01(depth / _settings.Swim.AscendDepthThreshold);
            Vector3 ascendForce = Vector3.up * _settings.Swim.AscendStrength * ascendFactor;

            _player.Rigidbody.AddForce(ascendForce, ForceMode.Force);
        }
    }
}