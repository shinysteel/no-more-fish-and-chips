using FishFlingers.Audio;
using FishFlingers.Cameras;
using PurrNet;
using ShinyOwl.Common;
using System;
using System.Globalization;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class RaftPlayerPhysicsModule : CharacterPhysicsModule
    {
        public RaftPlayer Player => (RaftPlayer)_entity;
        public RaftPlayerPhysicsSettings PlayerPhysicsSettings => (RaftPlayerPhysicsSettings)_entityPhysicsSettings;

        private float _jumpTimer;
        private bool _jumpRequest;
        
        public RaftPlayerPhysicsModule(RaftPlayer player, Rigidbody rigidbody) : base(player, rigidbody)
        { }

        public override void Tick()
        {
            if (!Player.isOwner)
            {
                return;
            }

            JumpTick();
        }

        public override void FixedTick()
        {
            base.FixedTick();

            if (!Player.isOwner)
            {
                return;
            }

            MoveFixedTick();
            LookFixedTick();
            JumpFixedTick();
            SwimFixedTick();
        }

        private void JumpTick()
        {
            _jumpTimer += Time.deltaTime;

            if (!Player.InputLogic.Jump)
            {
                return;
            }

            if (_jumpTimer < PlayerPhysicsSettings.Jump.Cooldown)
            {
                return;
            }

            // Jump on the next physics step
            _jumpRequest = true;
        }

        private void MoveFixedTick()
        {
            if (Player.AttackLogic.AttackState == RaftPlayerAttackState.Impact)
            {
                return;
            }

            Vector3 targetVelocity = Player.InputLogic.MoveDirection * PlayerPhysicsSettings.Move.Speed;

            if (Player.AttackLogic.AttackState == RaftPlayerAttackState.Windup)
            {
                targetVelocity *= PlayerPhysicsSettings.Move.AttackWindupMultiplier;
            }
            else if (Player.AttackLogic.AttackState == RaftPlayerAttackState.Impact)
            {
                targetVelocity *= PlayerPhysicsSettings.Move.AttackImpactMultiplier;
            }

            targetVelocity.y = _rigidbody.linearVelocity.y;

            float speed = Player.InputLogic.MoveDirection != Vector3.zero ? PlayerPhysicsSettings.Move.Acceleration : PlayerPhysicsSettings.Move.Deceleration;

            _rigidbody.linearVelocity = Vector3.MoveTowards(_rigidbody.linearVelocity, targetVelocity, speed * Time.fixedDeltaTime);
        }

        private void LookFixedTick()
        {
            Vector3 direction = Player.InputLogic.MoveDirection;

            if (direction == Vector3.zero)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            float speed = PlayerPhysicsSettings.Look.Speed;

            if (Player.AttackLogic.AttackState == RaftPlayerAttackState.Impact)
            {
                speed *= PlayerPhysicsSettings.Look.AttackImpactMultiplier;
            }

            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, targetRotation, speed * Time.fixedDeltaTime));
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
            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
            _rigidbody.AddForce(Vector3.up * PlayerPhysicsSettings.Jump.Strength, ForceMode.Impulse);

            _audioManager.PlaySound(SoundId.Jump);
        }

        private void SwimFixedTick()
        {
            // While in water, the player can hold spacebar to propel themselves up
            if (!Player.InputLogic.Ascend)
            {
                return;
            }

            if (!InWater)
            {
                return;
            }

            Collider waterCollider = _inWaterCollidersNonAlloc[0];

            Physics.ComputePenetration(Player.CapsuleCollider, _rigidbody.position, _rigidbody.rotation, waterCollider, waterCollider.transform.position, waterCollider.transform.rotation, out _, out float depth);

            float ascendFactor = Mathf.Clamp01(depth / PlayerPhysicsSettings.Swim.AscendDepthThreshold);
            Vector3 ascendForce = Vector3.up * PlayerPhysicsSettings.Swim.AscendStrength * ascendFactor;

            _rigidbody.AddForce(ascendForce, ForceMode.Force);
        }
    }
}