using FishFlingers.Cameras;
using FishFlingers.Environments;
using FishFlingers.Networking;
using FishFlingers.States;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Entities
{
    public abstract class NetEntity : NetBehaviour, IEntity
    {
        // Start of IEntity

        protected GameplayContext _context;

        public virtual void Initialise(GameplayContext context)
        {
            _context = context;
        }

        [SerializeField] protected EntityData _entityData;
        public EntityData EntityData => _entityData;

        private SyncVar<int> _currentHealth;

        protected HealthModule _healthModule;

        public int CurrentHealth => _healthModule.Current;
        public int MaxHealth => _healthModule.Max;

        public void SetHealth(int health)
        {
            _healthModule.SetHealth(health);
        }

        protected virtual void OnHealthChanged(int previous, int current) { }

        [SerializeField] protected Rigidbody _rigidbody;

        public Rigidbody Rigidbody => _rigidbody;

        // End of IEntity

        protected CameraManager _cameraManager;

        protected override void OnInitializeModules()
        {
            base.OnInitializeModules();

            _cameraManager = GameManager.Instance.Get<CameraManager>();

            _currentHealth = new SyncVar<int>(_entityData.Health);

            _healthModule = new HealthModule(_entityData.Health,
                getter: () => _currentHealth.value,
                setter: (int health) => _currentHealth.value = health,
                onChanged: OnHealthChanged);
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (!isServer)
            {
                return;
            }

            SetHealth(_entityData.Health);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _context = null;
        }
    }
}