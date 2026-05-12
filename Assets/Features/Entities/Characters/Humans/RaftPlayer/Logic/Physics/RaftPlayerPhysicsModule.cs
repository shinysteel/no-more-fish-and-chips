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
        private RaftPlayer _player;
        private CapsuleCollider _capsuleCollider;
        private RaftPlayerPhysicsSettings _settings;

        private float _jumpTimer;
        private bool _jumpRequest;

        private bool _isSwimClimbing;
        private RaycastHit[] _swimClimbHitsNonAlloc = new RaycastHit[5];

        public RaftPlayerPhysicsModule(RaftPlayer player, Rigidbody rigidbody, CapsuleCollider capsuleCollider) : base(player, rigidbody, capsuleCollider)
        {
            _player = player;
            _capsuleCollider = capsuleCollider;
            _settings = (RaftPlayerPhysicsSettings)_player.EntityDefinitionData.EntityPhysicsSettings;
        }

        public override void Tick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            JumpTick();
        }

        public override void FixedTick()
        {
            base.FixedTick();

            if (!_player.isOwner)
            {
                return;
            }

            MoveFixedTick();
            LookFixedTick();
            JumpFixedTick();
            SwimClimbFixedTick();
        }

        private void JumpTick()
        {
            _jumpTimer += Time.deltaTime;

            if (!_player.CanAct)
            {
                return;
            }

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

            Vector3 direction = _player.CanAct ? _player.InputLogic.MoveDirection : Vector3.zero;
            Vector3 targetVelocity = direction * _settings.Move.Speed;

            if (_player.AttackLogic.AttackState == RaftPlayerAttackState.Windup)
            {
                targetVelocity *= _settings.Move.AttackWindupMultiplier;
            }
            else if (_player.AttackLogic.AttackState == RaftPlayerAttackState.Impact)
            {
                targetVelocity *= _settings.Move.AttackImpactMultiplier;
            }

            targetVelocity.y = _rigidbody.linearVelocity.y;

            float speed = direction != Vector3.zero ? _settings.Move.Acceleration : _settings.Move.Deceleration;

            _rigidbody.linearVelocity = Vector3.MoveTowards(_rigidbody.linearVelocity, targetVelocity, speed * Time.fixedDeltaTime);
        }

        private void LookFixedTick()
        {
            Vector3 direction = _player.CanAct ? _player.InputLogic.MoveDirection : Vector3.zero;

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

            if (!_player.CanAct)
            {
                return;
            }

            if (!_isGrounded)
            {
                return;
            }

            // Cancel out gravity
            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
            _rigidbody.AddForce(Vector3.up * _settings.Jump.Strength, ForceMode.Impulse);

            _audioManager.PlaySound(SoundId.HumanJump);
        }

        private void SwimClimbFixedTick()
        {
            Vector3 direction = _player.CanAct ? _player.InputLogic.MoveDirection : Vector3.zero;

            if (direction == Vector3.zero)
            {
                _isSwimClimbing = false;
                return;
            }

            if (!InWater)
            {
                // A minimum launch force guarentees the player can climb back up even if they haven't built up much acceleration
                if (_isSwimClimbing && _rigidbody.linearVelocity.y < _settings.SwimClimb.LaunchStrength)
                {
                    Vector3 velocity = _rigidbody.linearVelocity;
                    velocity.y = _settings.SwimClimb.LaunchStrength;
                    _rigidbody.linearVelocity = velocity;
                }

                _isSwimClimbing = false;
                return;
            }

            Vector3 center = _rigidbody.position + _capsuleCollider.transform.TransformVector(_capsuleCollider.center);
            float radius = _capsuleCollider.radius * Mathf.Max(_player.transform.lossyScale.x, _player.transform.lossyScale.z);
            float height = Mathf.Max(_capsuleCollider.height * _capsuleCollider.transform.lossyScale.y, radius * 2f);
            float offset = height * 0.5f - radius;
            
            Vector3 point1 = center - Vector3.up * offset;
            Vector3 point2 = center + Vector3.up * offset;

            _isSwimClimbing = Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, _swimClimbHitsNonAlloc, _capsuleCollider.radius * 0.5f, _settings.SwimClimb.Mask) > 0;
            
            if (!_isSwimClimbing)
            {
                return;
            }

            _rigidbody.AddForce(Vector3.up * _settings.SwimClimb.ClimbSpeed, ForceMode.Acceleration);
        }
    }
}