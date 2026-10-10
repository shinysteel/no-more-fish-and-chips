using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Cameras;
using PurrNet;
using ShinyOwl.Common;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class EntityPhysicsLogic : EntityLogic
    {
        protected Rigidbody _rigidbody;
        protected Collider _collider;

        private EntityPhysicsSettings _settings;

        public Rigidbody Rigidbody => _rigidbody;
        public Collider Collider => _collider;

        public EntityPhysicsLogic(Entity entity, Rigidbody rigidbody, Collider collider) : base(entity)
        {
            _rigidbody = rigidbody;
            _collider = collider;

            _settings = _entity.EntityDefinitionData.EntityPhysicsSettings;
        }

        public override void OnSpawned()
        {
            if (!_entity.isOwner)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.constraints = RigidbodyConstraints.None;
            }
        }
    }
}