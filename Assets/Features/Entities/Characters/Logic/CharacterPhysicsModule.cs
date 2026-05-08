using FishFlingers.Audio;
using ShinyOwl.Common;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.Entities
{
    public class CharacterPhysicsModule : EntityPhysicsModule
    {
        public Character Character => (Character)_entity;
        public CharacterPhysicsSettings CharacterPhysicsSettings => (CharacterPhysicsSettings)_entityPhysicsSettings;

        protected bool _isGrounded;
        public bool IsGrounded => _isGrounded;

        private float _timeInWater;
        public float TimeInWater => _timeInWater;

        public bool InWater => _timeInWater > 0f;

        private RaycastHit[] _isGroundedHitsNonAlloc = new RaycastHit[2];
        protected Collider[] _inWaterCollidersNonAlloc = new Collider[1];

        public CharacterPhysicsModule(Character character, Rigidbody rigidbody) : base(character, rigidbody)
        { }

        public override void FixedTick()
        {
            IsGroundedFixedTick();
            InWaterFixedTick();
        }

        private void IsGroundedFixedTick()
        {
            Vector3 origin = Character.CharacterCollider.bounds.center;
            origin.y = Character.CharacterCollider.bounds.min.y;
            origin += Vector3.up * CharacterPhysicsSettings.ContactDetection.GroundCastRadius;

            int hits = Physics.SphereCastNonAlloc(origin, CharacterPhysicsSettings.ContactDetection.GroundCastRadius, Vector3.down, _isGroundedHitsNonAlloc, CharacterPhysicsSettings.ContactDetection.GroundCastDistance, CharacterPhysicsSettings.ContactDetection.GroundMask);

            bool isGrounded = false;

            for (int i = 0; i < hits; i++)
            {
                // Since we include the player layer to jump on other player's heads, we need to ignore our own collider here
                if (_isGroundedHitsNonAlloc[i].collider.gameObject != Character.gameObject)
                {
                    isGrounded = true;
                    break;
                }
            }

            _isGrounded = isGrounded;
        }
        
        private void InWaterFixedTick()
        {
            float radius = Vector3.Distance(Character.CharacterCollider.bounds.center, Character.CharacterCollider.bounds.min) * 0.5f;

            // If we are overlapping a collider on the water mask, we are in water
            bool inWater = Physics.OverlapSphereNonAlloc(Character.CharacterCollider.bounds.center, radius, _inWaterCollidersNonAlloc, CharacterPhysicsSettings.ContactDetection.WaterMask) > 0;

            if (inWater)
            {
                _timeInWater += Time.fixedDeltaTime;
            }
            else
            {
                _timeInWater = 0f;
            }
        }
    }
}