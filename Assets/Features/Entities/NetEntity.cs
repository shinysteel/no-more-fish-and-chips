using FishFlingers.Cameras;
using FishFlingers.Environments;
using FishFlingers.Networking;
using FishFlingers.States;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Entities
{
    // Maybe it's not so obvious that NetEntity is linked to the GameplayState,
    // but for now they aren't used in any other state
    public abstract class NetEntity : GameplayBehaviour, IEntity
    {
        // Start of IEntity

        [SerializeField] protected EntityData _entityData;
        public EntityData EntityData => _entityData;

        [SerializeField] protected EntityModel _entityModel;
        public EntityModel EntityModel => _entityModel;

        private SyncVar<int> _currentHealth;

        protected EntityHealthModule _healthModule;

        public EntityHealthModule HealthModule => _healthModule;

        public Transform Transform => transform;

        [SerializeField] protected Rigidbody _rigidbody;
        public Rigidbody Rigidbody => _rigidbody;

        // End of IEntity

        protected override void OnInitializeModules()
        {
            _currentHealth = new SyncVar<int>(_entityData.Health);

            _healthModule = new EntityHealthModule(this,
                getter: () => _currentHealth.value,
                setter: (int health) => _currentHealth.value = health);
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (isServer)
            {
                _healthModule.SetHealth(_entityData.Health);
            }
            
            _entityManager.RaiseNetEntitySpawned(this);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _entityManager?.RaiseNetEntityDespawned(this);

            _context = null;

            _healthModule = null;
        }
    }
}