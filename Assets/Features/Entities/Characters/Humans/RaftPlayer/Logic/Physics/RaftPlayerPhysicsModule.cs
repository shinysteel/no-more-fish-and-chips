using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Cameras;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerPhysicsModule : CharacterPhysicsModule
    {
        private RaftPlayer _player;
        private CapsuleCollider _capsuleCollider;
        private RaftPlayerPhysicsSettings _settings;

        private float _jumpTimer;
        private bool _jumpRequest;

        private bool _isClimbing;

        private RaycastHit[] _climbHitsNonAlloc = new RaycastHit[5];

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
            ClimbFixedTick();
        }

        private void MoveFixedTick()
        {
            Vector3 direction = _player.CanAct ? _player.InputLogic.MoveDirection : Vector3.zero;
            Vector3 targetVelocity = direction * _settings.Move.Speed;

            targetVelocity.y = _rigidbody.linearVelocity.y;

            float speed = direction != Vector3.zero ? _settings.Move.Acceleration : _settings.Move.Deceleration;

            _rigidbody.linearVelocity = Vector3.MoveTowards(_rigidbody.linearVelocity, targetVelocity, speed * Time.fixedDeltaTime);
        }

        private void LookFixedTick()
        {
            Vector3 direction;

            if (!_player.CanAct)
            {
                direction = Vector3.zero;
            }
            else if (!_player.AttackLogic.IsAttacking)
            {
                direction = _player.InputLogic.MoveDirection;
            }
            else
            {
                Ray ray = _cameraManager.CinemachineBrain.OutputCamera.ScreenPointToRay(_player.InputLogic.GameplayMouse);

                // Have the plane sit at the player's origin so that y does not influence the target
                Plane plane = new Plane(Vector3.up, _player.transform.position);

                // Face the cursor
                plane.Raycast(ray, out float distance);

                direction = (ray.GetPoint(distance) - _player.transform.position).normalized;
            }

            if (direction == Vector3.zero)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            float speed = _settings.Look.Speed;

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

            if (_groundSurface == null)
            {
                return;
            }

            // Cancel out gravity
            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
            _rigidbody.AddForce(Vector3.up * _settings.Jump.Strength, ForceMode.Impulse);

            _player.AnimateLogic.Jump();
        }

        private void ClimbFixedTick()
        {
            Vector3 direction = _player.CanAct ? _player.InputLogic.MoveDirection : Vector3.zero;

            if (direction == Vector3.zero)
            {
                _isClimbing = false;
                return;
            }

            if (!InWater)
            {
                // A minimum launch force guarentees the player can climb back up even if they haven't built up much acceleration
                if (_isClimbing && _rigidbody.linearVelocity.y < _settings.Climb.LaunchStrength)
                {
                    Vector3 velocity = _rigidbody.linearVelocity;
                    velocity.y = _settings.Climb.LaunchStrength;
                    _rigidbody.linearVelocity = velocity;
                }

                _isClimbing = false;
                return;
            }

            _isClimbing = Utils.Physics.CapsuleCastNonAlloc(_capsuleCollider, Vector3.zero, direction, _climbHitsNonAlloc, _capsuleCollider.radius * 0.5f, _settings.Climb.Mask) > 0;

            if (!_isClimbing)
            {
                return;
            }

            _rigidbody.AddForce(Vector3.up * _settings.Climb.ClimbSpeed, ForceMode.Acceleration);
        }
    }
}