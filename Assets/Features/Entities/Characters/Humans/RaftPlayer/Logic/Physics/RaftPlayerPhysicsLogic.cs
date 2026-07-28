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
    public class RaftPlayerPhysicsLogic : CharacterPhysicsLogic
    {
        private RaftPlayer _player;
        private CapsuleCollider _capsuleCollider;
        private RaftPlayerPhysicsSettings _settings;

        private float _jumpTimer;
        private bool _jumpRequest;

        private bool _isClimbing;

        private RaycastHit[] _climbHitsNonAlloc = new RaycastHit[5];

        public RaftPlayerPhysicsLogic(RaftPlayer player, Rigidbody rigidbody, NetworkRigidbody networkRigidbody, CapsuleCollider capsuleCollider) : base(player, rigidbody, networkRigidbody, capsuleCollider)
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

            if (!_player.CharacterActLogic.CanAct)
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
            Vector3 direction = _player.CharacterActLogic.CanAct ? _player.InputLogic.MoveDirection : Vector3.zero;
            Vector3 targetVelocity = direction * _settings.Move.Speed;

            targetVelocity.y = _networkRigidbody.linearVelocity.y;

            float speed = direction != Vector3.zero ? _settings.Move.Acceleration : _settings.Move.Deceleration;

            _networkRigidbody.linearVelocity = Vector3.MoveTowards(_networkRigidbody.linearVelocity, targetVelocity, speed * Time.fixedDeltaTime);
        }

        private void LookFixedTick()
        {
            Vector3 direction;

            if (!_player.CharacterActLogic.CanAct)
            {
                direction = Vector3.zero;
            }
            else if (!_player.AttackLogic.IsAttacking)
            {
                direction = _player.InputLogic.MoveDirection;
            }
            else
            {
                Ray ray = _cameraManager.CinemachineBrain.OutputCamera.ScreenPointToRay(_player.InputLogic.Mouse);

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

            _networkRigidbody.MoveRotation(Quaternion.Slerp(_networkRigidbody.rotation, targetRotation, speed * Time.fixedDeltaTime));
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

            if (!_player.CharacterActLogic.CanAct)
            {
                return;
            }

            if (_groundSurface == null)
            {
                return;
            }

            // Cancel out gravity
            _networkRigidbody.linearVelocity = new Vector3(_networkRigidbody.linearVelocity.x, 0f, _networkRigidbody.linearVelocity.z);
            _networkRigidbody.AddForce(Vector3.up * _settings.Jump.Strength, ForceMode.Impulse);

            _player.AnimateLogic.Jump();
        }

        private void ClimbFixedTick()
        {
            Vector3 direction = _player.CharacterActLogic.CanAct ? _player.InputLogic.MoveDirection : Vector3.zero;

            if (direction == Vector3.zero)
            {
                _isClimbing = false;
                return;
            }

            if (!InWater)
            {
                // A minimum launch force guarentees the player can climb back up even if they haven't built up much acceleration
                if (_isClimbing && _networkRigidbody.linearVelocity.y < _settings.Climb.LaunchStrength)
                {
                    Vector3 velocity = _networkRigidbody.linearVelocity;
                    velocity.y = _settings.Climb.LaunchStrength;
                    _networkRigidbody.linearVelocity = velocity;
                }

                _isClimbing = false;
                return;
            }

            _isClimbing = Utils.Physics.CapsuleCastNonAlloc(_capsuleCollider, Vector3.zero, direction, _climbHitsNonAlloc, _capsuleCollider.radius * 0.5f, _settings.Climb.Mask) > 0;

            if (!_isClimbing)
            {
                return;
            }

            _networkRigidbody.AddForce(Vector3.up * _settings.Climb.ClimbSpeed, ForceMode.Acceleration);
        }
    }
}