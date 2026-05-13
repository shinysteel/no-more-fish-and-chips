using FishFlingers.Audio;
using ShinyOwl.Common;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.Entities
{
    public class CharacterPhysicsModule : EntityPhysicsModule
    {
        private Character _character;
        private CharacterPhysicsSettings _settings;

        protected bool _isGrounded;
        public bool IsGrounded => _isGrounded;

        private float _timeInWater;
        public float TimeInWater => _timeInWater;

        public bool InWater => _timeInWater > 0f;

        public bool InAir => !_isGrounded && !InWater;

        private RaycastHit[] _isGroundedHitsNonAlloc = new RaycastHit[2];
        protected Collider[] _inWaterCollidersNonAlloc = new Collider[1];

        public CharacterPhysicsModule(Character character, Rigidbody rigidbody, Collider collider) : base(character, rigidbody, collider)
        {
            _character = character;
            _settings = (CharacterPhysicsSettings)_character.EntityDefinitionData.EntityPhysicsSettings;
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
            origin += Vector3.up * _settings.ContactDetection.GroundCastRadius;

            int hits = Physics.SphereCastNonAlloc(origin, _settings.ContactDetection.GroundCastRadius, Vector3.down, _isGroundedHitsNonAlloc, _settings.ContactDetection.GroundCastDistance, _settings.ContactDetection.GroundMask);

            bool isGrounded = false;

            for (int i = 0; i < hits; i++)
            {
                // Since we include the player layer to jump on other player's heads, we need to ignore our own collider here
                if (_isGroundedHitsNonAlloc[i].collider.gameObject != _character.gameObject)
                {
                    isGrounded = true;
                    break;
                }
            }

            _isGrounded = isGrounded;
        }
        
        private void InWaterFixedTick()
        {
            float radius = Vector3.Distance(_collider.bounds.center, _collider.bounds.min) * 0.5f;

            // If we are overlapping a collider on the water mask, we are in water
            bool inWater = Physics.OverlapSphereNonAlloc(_collider.bounds.center, radius, _inWaterCollidersNonAlloc, _settings.ContactDetection.WaterMask) > 0;

            if (inWater)
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