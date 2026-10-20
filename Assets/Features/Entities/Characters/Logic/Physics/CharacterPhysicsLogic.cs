using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Environments;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.Entities
{
    public class CharacterPhysicsLogic : EntityPhysicsLogic
    {
        private Character _character;
        protected CapsuleCollider _capsuleCollider;
        public CapsuleCollider CapsuleCollider => _capsuleCollider;

        private CharacterPhysicsSharedSettings _sharedSettings;

        protected ISurface _groundSurface;
        public ISurface GroundSurface => _groundSurface;

        // Last non-null value
        private ISurface _lastGroundSurface;
        public ISurface LastGroundSurface => _lastGroundSurface;

        private float _timeInWater;
        public float TimeInWater => _timeInWater;

        public bool IsGrounded => _groundSurface != null;
        public bool InWater => _timeInWater > 0f;
        public bool InAir => _groundSurface == null && _timeInWater == 0f;

        private RaycastHit[] _isGroundedHitsNonAlloc = new RaycastHit[2];

        public CharacterPhysicsLogic(Character character, Rigidbody rigidbody, CapsuleCollider collider) : base(character, rigidbody, collider)
        {
            _character = character;
            _capsuleCollider = collider;

            _sharedSettings = _character.CharacterDefinitionData.CharacterPhysicsSharedSettings;
        }

        public void Move(Vector3 direction, float speed, float acceleration)
        {
            Vector3 targetVelocity = direction * speed;

            Vector3 velocity = _rigidbody.linearVelocity;
            velocity.y = 0f;
            Vector3 clampedVelocity = Vector3.ClampMagnitude(velocity, speed);

            Vector3 delta = targetVelocity - clampedVelocity;
            float maxDelta = acceleration * Time.fixedDeltaTime;
            delta = Vector3.ClampMagnitude(delta, maxDelta);

            _rigidbody.AddForce(delta / Time.fixedDeltaTime, ForceMode.Acceleration);
        }

        public void Rotate(Vector3 direction, float speed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, targetRotation, speed * Time.fixedDeltaTime));
        }

        public override void FixedTick()
        {
            IsGroundedFixedTick();
            InWaterFixedTick();
        }

        private void IsGroundedFixedTick()
        {
            Vector3 origin = _collider.bounds.center;
            origin.y = _collider.bounds.min.y;
            origin += Vector3.up * _sharedSettings.ContactDetection.GroundCastRadius;

            int hits = Physics.SphereCastNonAlloc(origin, _sharedSettings.ContactDetection.GroundCastRadius, Vector3.down, _isGroundedHitsNonAlloc, _sharedSettings.ContactDetection.GroundCastDistance, _sharedSettings.ContactDetection.GroundMask);

            _groundSurface = null;
            
            for (int i = 0; i < hits; i++)
            {
                // Since we include the player layer to jump on other player's heads, we need to ignore our own collider here
                if (_isGroundedHitsNonAlloc[i].collider == _collider)
                {
                    continue;
                }

                // Anything hit on this layer mask needs to implement ISurface
                _groundSurface = _isGroundedHitsNonAlloc[i].collider.GetComponent<ISurface>();
                _lastGroundSurface = _groundSurface;
            }
        }
        
        private void InWaterFixedTick()
        {
            // If we are overlapping a collider on the water mask, we are in water
            if (_context.Ocean.InWater(_collider))
            {
                _timeInWater += Time.fixedDeltaTime;
            }
            else
            {
                ResetTimeInWater();
            }
        }

        public void ResetTimeInWater()
        {
            _timeInWater = 0f;
        }
    }
}