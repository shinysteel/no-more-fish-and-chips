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
        [SerializeField] protected EntityDefinitionData _entityDefinitionData;
        [SerializeField] protected EntityModel _entityModel;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Collider _collider;

        protected NetworkManager _networkManager;
        protected EntityManager _entityManager;

        protected GameplayContext _context;

        protected bool _isSpawned;
        protected int _currentHealth;
        protected bool _isDefeated;

        protected EntityHealthModule _entityHealthModule;
        protected EntityDefeatModule _entityDefeatModule;
        protected EntityLifecycleModule _entityLifecycleModule;
        protected EntityEffectsModule _entityEffectsModule;
        protected EntityPhysicsModule _entityPhysicsModule;

        public EntityDefinitionData EntityDefinitionData => _entityDefinitionData;
        public EntityModel EntityModel => _entityModel;

        public bool IsSpawned => _isSpawned;
        public bool IsOwner => true;
        public Transform Transform => transform;

        public EntityHealthModule EntityHealthModule => _entityHealthModule;
        public EntityDefeatModule EntityDefeatModule => _entityDefeatModule;
        public EntityLifecycleModule EntityLifecycleModule => _entityLifecycleModule;
        public EntityEffectsModule EntityEffectsModule => _entityEffectsModule;
        public EntityPhysicsModule EntityPhysicsModule => _entityPhysicsModule;

        protected virtual void Awake()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();
        }

        public virtual void Initialise(GameplayContext context)
        {
            _context = context;

            if (_networkManager.IsServer)
            {
                _entityHealthModule.SetHealth(_entityDefinitionData.Health);
            }
        }

        protected virtual void Update()
        {
            if (!_isSpawned)
            {
                return;
            }

            _entityDefeatModule.Tick();
        }

        protected virtual void FixedUpdate()
        {
            if (!_isSpawned)
            {
                return;
            }

            _entityPhysicsModule.FixedTick();
        }

        public virtual void OnTakenFromPool()
        {
            _entityHealthModule = new EntityHealthModule(this,
                getter: () => _currentHealth,
                setter: HealthModuleSetter);

            _entityDefeatModule ??= new EntityDefeatModule(this,
                isDefeatedGetter: () => _isDefeated,
                isDefeatedSetter: DefeatModuleSetter);

            _entityLifecycleModule = new EntityLifecycleModule(this);

            _entityEffectsModule ??= new EntityEffectsModule(this);

            _entityPhysicsModule ??= new EntityPhysicsModule(this, _rigidbody, _collider);

            _isSpawned = true;

            _entityManager.RaiseNetEntitySpawned(this);
        }

        public virtual void OnReturnedToPool()
        {
            _entityManager?.RaiseNetEntityDespawned(this);

            _isSpawned = false;

            _context = null;

            _entityHealthModule = null;
            _entityDefeatModule = null;
            _entityLifecycleModule = null;
            _entityEffectsModule = null;
            _entityPhysicsModule = null;
        }

        public void SetHealth(int health)
        {
            if (_currentHealth == health)
            {
                return;
            }

            int previous = _currentHealth;
            _currentHealth = health;
            _entityHealthModule.HandleChanged(previous, _currentHealth);
        }

        protected abstract void HealthModuleSetter(int health);

        private void DefeatModuleSetter(bool defeated)
        {
            _isDefeated = defeated;
            _entityDefeatModule.HandleIsDefeatedChanged(_isDefeated);
        }
    }
}