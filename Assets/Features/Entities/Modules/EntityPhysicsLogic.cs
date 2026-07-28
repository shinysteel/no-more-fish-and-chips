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
        protected CameraManager _cameraManager;
        protected AudioManager _audioManager;

        protected Rigidbody _rigidbody;
        protected NetworkRigidbody _networkRigidbody;
        protected Collider _collider;

        private EntityPhysicsSettings _settings;

        public Rigidbody Rigidbody => _rigidbody;
        public NetworkRigidbody NetworkRigidbody => _networkRigidbody;
        public Collider Collider => _collider;

        public EntityPhysicsLogic(Entity entity, Rigidbody rigidbody, NetworkRigidbody networkRigidbody, Collider collider) : base(entity)
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _audioManager = GameManager.Instance.Get<AudioManager>();

            _rigidbody = rigidbody;
            _networkRigidbody = networkRigidbody;
            _collider = collider;

            _settings = _entity.EntityDefinitionData.EntityPhysicsSettings;
        }

        public override void OnSpawned()
        {
            if (!_entity.isOwner)
            {
                _networkRigidbody.isKinematic = true;
            }
        }
    }
}