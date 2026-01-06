using FishFlingers.Cameras;
using FishFlingers.Environments;
using FishFlingers.Networking;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;

using NetworkManager = FishFlingers.Networking.NetworkManager;

namespace FishFlingers.Entities
{
    public abstract class NetEntity : NetworkBehaviour, IEntity
    {
        // Start of IEntity

        protected Raft _raft;

        public void Initialise(Raft raft)
        {
            _raft = raft;
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

        protected NetworkManager _networkManager;
        protected CameraManager _cameraManager;

        protected override void OnInitializeModules()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _cameraManager = GameManager.Instance.Get<CameraManager>();

            _currentHealth = new SyncVar<int>(_maxHealth);

            _healthModule = new HealthModule(_maxHealth,
                getter: () => _currentHealth.value,
                setter: (int health) => _currentHealth.value = health,
                onChanged: OnHealthChanged);
        }

        protected override void OnSpawned()
        {
            if (!isServer)
            {
                return;
            }

            SetHealth(_maxHealth);
        }

        protected override void OnDespawned()
        {
            _raft = null;
        }
    }
}