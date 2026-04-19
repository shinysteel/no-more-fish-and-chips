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

        protected EntityHealthModule _entityHealthModule;

        public int CurrentHealth => _entityHealthModule.Current;
        public int MaxHealth => _entityHealthModule.Max;

        public void SetHealth(int health)
        {
            _entityHealthModule.SetHealth(health);
        }

        protected virtual void OnHealthChanged(int previous, int current) { }

        [SerializeField] protected Rigidbody _rigidbody;

        public Rigidbody Rigidbody => _rigidbody;

        // End of IEntity

        protected override void OnInitializeModules()
        {
            _currentHealth = new SyncVar<int>(_entityData.Health);

            _entityHealthModule = new EntityHealthModule(_entityData.Health,
                getter: () => _currentHealth.value,
                setter: (int health) => _currentHealth.value = health,
                onChanged: OnHealthChanged);
        }

        protected override void OnSpawned()
        {
            Log.Info($"{name} raises spawned");

            base.OnSpawned();

            if (isServer)
            {
                SetHealth(_entityData.Health);
            }
            
            _entityManager.RaiseNetEntitySpawned(this);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _entityManager?.RaiseNetEntityDespawned(this);

            _context = null;
        }
    }
}