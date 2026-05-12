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
        [SerializeField] protected EntityDefinitionData _entityDefinitionData;
        [SerializeField] protected EntityModel _entityModel;
        [SerializeField] protected Rigidbody _rigidbody;
        [SerializeField] protected Collider _collider;

        private SyncVar<int> _netCurrentHealth;
        private SyncVar<bool> _netIsDefeated;

        protected EntityHealthModule _entityHealthModule;
        protected EntityDefeatModule _entityDefeatModule;
        protected EntityLifecycleModule _entityLifecycleModule;
        protected EntityEffectsModule _entityEffectsModule;
        protected EntityPhysicsModule _entityPhysicsModule;

        public EntityDefinitionData EntityDefinitionData => _entityDefinitionData;
        public EntityModel EntityModel => _entityModel;

        public new bool IsSpawned => isSpawned;
        public bool IsOwner => isOwner;
        public Transform Transform => transform;

        public EntityHealthModule EntityHealthModule => _entityHealthModule;
        public EntityDefeatModule EntityDefeatModule => _entityDefeatModule;
        public EntityLifecycleModule EntityLifecycleModule => _entityLifecycleModule;
        public EntityEffectsModule EntityEffectsModule => _entityEffectsModule;
        public EntityPhysicsModule EntityPhysicsModule => _entityPhysicsModule;

        protected override void OnInitializeModules()
        {
            _netCurrentHealth = new SyncVar<int>(_entityDefinitionData.Health, ownerAuth: true);
            _netIsDefeated = new SyncVar<bool>(ownerAuth: true);

            _netCurrentHealth.onChangedWithOld += HandleNetCurrentHealthChanged;
            _netIsDefeated.onChanged += HandleNetIsDefeatedChanged;

            _entityHealthModule = new EntityHealthModule(this,
                getter: () => _netCurrentHealth.value,
                setter: SetNetCurrentHealthRpc);

            _entityDefeatModule = CreateDefeatModule();

            _entityLifecycleModule = new EntityLifecycleModule(this);

            _entityEffectsModule = CreateEffectsModule();

            _entityPhysicsModule = CreatePhysicsModule();
        }

        protected virtual EntityDefeatModule CreateDefeatModule()
        {
            return new EntityDefeatModule(this, GetNetIsDefeated, SetNetIsDefeated);
        }

        protected virtual EntityEffectsModule CreateEffectsModule()
        {
            return new EntityEffectsModule(this);
        }

        protected virtual EntityPhysicsModule CreatePhysicsModule()
        {
            return new EntityPhysicsModule(this, _rigidbody, _collider);
        }

        protected bool GetNetIsDefeated()
        {
            return _netIsDefeated.value;
        }

        protected void SetNetIsDefeated(bool isDefeated)
        {
            _netIsDefeated.value = isDefeated;
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (isServer)
            {
                _entityHealthModule.SetHealth(_entityDefinitionData.Health);
            }
            
            _entityManager.RaiseNetEntitySpawned(this);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _entityManager?.RaiseNetEntityDespawned(this);

            _netCurrentHealth.onChangedWithOld -= HandleNetCurrentHealthChanged;
            _netIsDefeated.onChanged -= HandleNetIsDefeatedChanged;

            _context = null;

            _entityHealthModule = null;
            _entityDefeatModule = null;
            _entityLifecycleModule = null;
            _entityEffectsModule = null;
            _entityPhysicsModule = null;
        }

        protected virtual void Update()
        {
            if (!isFullySpawned)
            {
                return;
            }

            _entityDefeatModule.Tick();
            _entityPhysicsModule.Tick();
        }

        protected virtual void FixedUpdate()
        {
            if (!isFullySpawned)
            {
                return;
            }

            _entityDefeatModule.FixedTick();
            _entityPhysicsModule.FixedTick();
        }

        private void HandleNetCurrentHealthChanged(int previous, int current)
        {
            _entityHealthModule.HandleChanged(previous, current);
        }

        private void HandleNetIsDefeatedChanged(bool defeated)
        {
            _entityDefeatModule.HandleIsDefeatedChanged(defeated);
        }

        [ServerRpc]
        private void SetNetCurrentHealthRpc(int health)
        {
            _netCurrentHealth.value = health;
        }

        [ObserversRpc]
        public void AnimateHurtRpc()
        {
            _entityEffectsModule.AnimateHurt();
        }

        [TargetRpc]
        public void AddForceRpc(PlayerID id, Vector3 force)
        {
            _entityPhysicsModule.Rigidbody.AddForce(force, ForceMode.Impulse);
        }

        [TargetRpc]
        public void AddTorqueRpc(PlayerID id, Vector3 torque)
        {
            _entityPhysicsModule.Rigidbody.AddTorque(torque, ForceMode.Impulse);
        }
    }
}