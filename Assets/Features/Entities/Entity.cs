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

        [SerializeField] protected EntityModel _entityModel;
        public EntityModel EntityModel => _entityModel;

        private int _currentHealth;

        protected EntityHealthModule _healthModule;

        public EntityHealthModule HealthModule => _healthModule;

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

        public virtual void OnTakenFromPool()
        {
            _healthModule = new EntityHealthModule(this,
                getter: () => _currentHealth,
                setter: (int health) =>
                {
                    int previous = _currentHealth;
                    _currentHealth = health;
                    _healthModule.RaiseChanged(previous, _currentHealth);
                });

            if (_networkManager.IsServer)
            {
                _healthModule.SetHealth(_entityData.Health);
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