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

        protected bool _isInitialised;
        protected GameplayContext _context;

        public void Initialise(GameplayContext context)
        {
            _isInitialised = true;
            _context = context;
        }

        [SerializeField] private int _maxHealth = 1;

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

            _currentHealth = new SyncVar<int>(_maxHealth);

            _healthModule = new HealthModule(_maxHealth,
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

            SetHealth(_maxHealth);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _isInitialised = false;
            _context = null;
        }
    }
}