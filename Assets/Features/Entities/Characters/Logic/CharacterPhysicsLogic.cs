using ShinyOwl.Common;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.Entities
{
    public class CharacterPhysicsLogic
    {
        private Character _character;

        private CharacterPhysicsSettings _settings;

        protected bool _isGrounded;
        public bool IsGrounded => _isGrounded;

        protected bool _inWater;
        public bool InWater => _inWater;

        private RaycastHit[] _isGroundedHitsNonAlloc = new RaycastHit[2];
        protected Collider[] _inWaterCollidersNonAlloc = new Collider[1];

        public CharacterPhysicsLogic(Character character)
        {
            _character = character;

            _settings = _character.CharacterData.CharacterPhysicsSettings;
        }

        public virtual void Tick()
        { }

        public virtual void FixedTick()
        {
            IsGroundedFixedTick();
            InWaterFixedTick();
        }

        private void IsGroundedFixedTick()
        {
            Vector3 origin = _character.CharacterCollider.bounds.center;
            origin.y = _character.CharacterCollider.bounds.min.y;
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
            float radius = Vector3.Distance(_character.CharacterCollider.bounds.center, _character.CharacterCollider.bounds.min);

            // If we are overlapping a collider on the water mask, we are in water
            _inWater = Physics.OverlapSphereNonAlloc(_character.CharacterCollider.bounds.center, radius, _inWaterCollidersNonAlloc, _settings.ContactDetection.WaterMask) > 0;
        }
    }
}