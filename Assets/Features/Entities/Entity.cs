using FishFlingers.Environments;
using FishFlingers.Networking;
using FishFlingers.Pools;
using UnityEngine;

namespace FishFlingers.Entities
{
    public abstract class Entity : MonoBehaviour, IEntity, IPoolable
    {
        // Start of IEntity

        protected Raft _raft;
        public virtual void Initialise(Raft raft)
        {
            _raft = raft;
        }

        [SerializeField] private int _maxHealth = 1;

        private int _currentHealth;

        protected HealthModule _healthModule;

        public int CurrentHealth => _healthModule.Current;
        public int MaxHealth => _healthModule.Max;

        public virtual void SetHealth(int health)
        {
            _healthModule.SetHealth(health);
        }

        protected virtual void OnHealthChanged(int previous, int current) { }


        [SerializeField] protected Rigidbody _rigidbody;

        public Rigidbody Rigidbody => _rigidbody;

        // End of IEntity

        protected NetworkManager _networkManager;

        protected virtual void Awake()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
        }

        public virtual void OnTakenFromPool()
        {
            if (!_networkManager.IsServer)
            {
                return;
            }

            _healthModule = new HealthModule(_maxHealth,
                getter: () => _currentHealth,
                setter: (int health) => _currentHealth = health,
                onChanged: OnHealthChanged);

            SetHealth(_maxHealth);
        }

        public virtual void OnReturnedToPool() 
        {
            _raft = null;

            _healthModule = null;
        }
    }
}