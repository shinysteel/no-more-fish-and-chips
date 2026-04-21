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

        protected EntityHealthModule _healthModule;
        protected EntityDefeatModule _defeatModule;
        protected EntityRagdollModule _ragdollModule;

        public EntityHealthModule HealthModule => _healthModule;
        public EntityDefeatModule DefeatModule => _defeatModule;
        public EntityRagdollModule RagdollModule => _ragdollModule;

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
            _healthModule = new EntityHealthModule(_entityData.Health,
                getter: () => _currentHealth,
                setter: (int health) => _currentHealth = health);

            _defeatModule = new EntityDefeatModule(this, _entityData.DefeatTime);

            _ragdollModule = new EntityRagdollModule(_rigidbody);

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
            _defeatModule = null;
            _ragdollModule = null;
        }
    }
}