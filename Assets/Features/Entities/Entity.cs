using FishFlingers.Environments;
using FishFlingers.Networking;
using FishFlingers.Pools;
using FishFlingers.States;
using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Entities
{
    public abstract class Entity : MonoBehaviour, IEntity, ITypedPoolable
    {
        // Start of IEntity

        protected GameplayContext _context;

        public virtual void Initialise(GameplayContext context)
        {
            _context = context;
            
            if (_networkManager.IsServer)
            {
                _healthModule.SetHealth(_entityDefinitionData.Health);
            }
        }

        [SerializeField] protected EntityDefinitionData _entityDefinitionData;
        public EntityDefinitionData EntityDefinitionData => _entityDefinitionData;

        [SerializeField] protected EntityModel _entityModel;
        public EntityModel EntityModel => _entityModel;

        private bool _isSpawned;
        public bool IsSpawned => _isSpawned;

        protected int _currentHealth;

        protected EntityHealthModule _healthModule;

        public EntityHealthModule HealthModule => _healthModule;

        protected EntityDefeatModule _defeatModule;
        public EntityDefeatModule DefeatModule => _defeatModule;

        private EntityLifecycleModule _lifecycleModule;
        public EntityLifecycleModule LifecycleModule => _lifecycleModule;

        public Transform Transform => transform;

        [SerializeField] protected Rigidbody _rigidbody;
        public Rigidbody Rigidbody => _rigidbody;

        void IEntity.AddForce(Vector3 force)
        {
            _rigidbody.AddForce(force);
        }

        void IEntity.AddTorque(Vector3 torque)
        {
            _rigidbody.AddTorque(torque);
        }

        // End of IEntity

        protected NetworkManager _networkManager;
        protected EntityManager _entityManager;

        protected virtual void Awake()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();
        }

        protected abstract void HealthModuleSetter(int health);

        public void SetHealth(int health)
        {
            if (_currentHealth == health)
            {
                return;
            }

            int previous = _currentHealth;
            _currentHealth = health;
            _healthModule.RaiseChanged(previous, _currentHealth);
        }

        protected virtual void Update()
        {
            if (!_isSpawned)
            {
                return;
            }

            _defeatModule.Tick();
        }

        public virtual void OnTakenFromPool()
        {
            _healthModule = new EntityHealthModule(this,
                getter: () => _currentHealth,
                setter: HealthModuleSetter);

            _defeatModule ??= new EntityDefeatModule(this);

            _lifecycleModule = new EntityLifecycleModule(this);

            _isSpawned = true;

            _entityManager.RaiseNetEntitySpawned(this);
        }

        public virtual void OnReturnedToPool()
        {
            _entityManager?.RaiseNetEntityDespawned(this);

            _isSpawned = false;

            _context = null;

            _healthModule = null;
            _defeatModule = null;
            _lifecycleModule = null;
        }
    }
}