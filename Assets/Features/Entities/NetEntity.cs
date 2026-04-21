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

        private SyncVar<int> _currentHealth;

        protected EntityHealthModule _healthModule;
        protected EntityDefeatModule _defeatModule;
        protected EntityRagdollModule _ragdollModule;

        public EntityHealthModule HealthModule => _healthModule;
        public EntityDefeatModule DefeatModule => _defeatModule;
        public EntityRagdollModule RagdollModule => _ragdollModule;

        [SerializeField] protected Rigidbody _rigidbody;

        public Rigidbody Rigidbody => _rigidbody;

        // End of IEntity

        protected override void OnInitializeModules()
        {
            _currentHealth = new SyncVar<int>(_entityData.Health);

            _healthModule = new EntityHealthModule(_entityData.Health,
                getter: () => _currentHealth.value,
                setter: (int health) => _currentHealth.value = health);

            _defeatModule = new EntityDefeatModule(this, _entityData.DefeatTime);

            _ragdollModule = new EntityRagdollModule(_rigidbody);
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (isServer)
            {
                _healthModule.OnChanged += HandleHealthChanged;
                _defeatModule.OnDefeated += HandleDefeated;

                _healthModule.SetHealth(_entityData.Health);
            }
            
            _entityManager.RaiseNetEntitySpawned(this);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _entityManager?.RaiseNetEntityDespawned(this);

            if (isServer)
            {
                _healthModule.OnChanged -= HandleHealthChanged;
                _defeatModule.OnDefeated -= HandleDefeated;

                _ragdollModule.SetEnabled(false);
            }

            _context = null;

            _healthModule = null;
            _defeatModule = null;
            _ragdollModule = null;
        }

        private void HandleHealthChanged(int previous, int current)
        {
            if (current == 0)
            {
                _defeatModule.Defeat();
            }
        }

        private void HandleDefeated()
        {
            _ragdollModule.SetEnabled(true);
        }
    }
}