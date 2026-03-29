using FishFlingers.Environments;
using FishFlingers.Networking;
using FishFlingers.Pools;
using FishFlingers.States;
using UnityEngine;

namespace FishFlingers.Entities
{
    public abstract class Entity : MonoBehaviour, IEntity, IPoolable
    {
        // Start of IEntity

        protected GameplayContext _context;

        public virtual void Initialise(GameplayContext context)
        {
            _context = context;
        }

        [SerializeField] protected EntityData _entityData;
        public EntityData EntityData => _entityData;

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
        protected EntityManager _entityManager;

        protected virtual void Awake()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();
        }

        public virtual void OnTakenFromPool()
        {
            _healthModule = new HealthModule(_entityData.Health,
                getter: () => _currentHealth,
                setter: (int health) => _currentHealth = health,
                onChanged: OnHealthChanged);

            if (_networkManager.IsServer)
            {
                SetHealth(_entityData.Health);
            }

            _entityManager.RaiseNetEntitySpawned(this);
        }

        public virtual void OnReturnedToPool()
        {
            _entityManager?.RaiseNetEntityDespawned(this);

            _context = null;

            _healthModule = null;
        }
    }
}