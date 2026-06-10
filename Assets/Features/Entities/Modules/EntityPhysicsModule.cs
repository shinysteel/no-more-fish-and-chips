using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Cameras;
using ShinyOwl.Common;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class EntityPhysicsModule
    {
        protected CameraManager _cameraManager;
        protected AudioManager _audioManager;

        private IEntity _entity;
        protected Rigidbody _rigidbody;
        protected Collider _collider;

        private EntityPhysicsSettings _settings;

        public Rigidbody Rigidbody => _rigidbody;
        public Collider Collider => _collider;

        public EntityPhysicsModule(IEntity entity, Rigidbody rigidbody, Collider collider)
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _audioManager = GameManager.Instance.Get<AudioManager>();

            _entity = entity;
            _rigidbody = rigidbody;
            _collider = collider;

            _settings = _entity.EntityDefinitionData.EntityPhysicsSettings;
        }

        public virtual void Tick()
        { }

        public virtual void FixedTick()
        { }
    }
}