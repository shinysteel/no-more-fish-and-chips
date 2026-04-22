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
        // Start of IEntity

        [SerializeField] protected EntityData _entityData;
        public EntityData EntityData => _entityData;

        [SerializeField] protected EntityModel _entityModel;
        public EntityModel EntityModel => _entityModel;

        private SyncVar<int> _netCurrentHealth;

        protected EntityHealthModule _healthModule;

        public EntityHealthModule HealthModule => _healthModule;

        public Transform Transform => transform;

        [SerializeField] protected Rigidbody _rigidbody;
        public Rigidbody Rigidbody => _rigidbody;

        void IEntity.AddForce(Vector3 force)
        {
            AddForceRpc(force);
        }

        void IEntity.AddTorque(Vector3 torque)
        {
            AddTorqueRpc(torque);
        }

        [ServerRpc]
        private void AddForceRpc(Vector3 force)
        {
            _rigidbody.AddForce(force, ForceMode.Impulse);
        }

        [ServerRpc]
        private void AddTorqueRpc(Vector3 torque)
        {
            _rigidbody.AddTorque(torque, ForceMode.Impulse);
        }

        // End of IEntity

        protected override void OnInitializeModules()
        {
            _netCurrentHealth = new SyncVar<int>(_entityData.Health);

            _netCurrentHealth.onChangedWithOld += HandleNetCurrentHealthChanged;

            _healthModule = new EntityHealthModule(this,
                getter: () => _netCurrentHealth.value,
                setter: SetHealthRpc);
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (isServer)
            {
                _healthModule.SetHealth(_entityData.Health);
            }
            
            _entityManager.RaiseNetEntitySpawned(this);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _entityManager?.RaiseNetEntityDespawned(this);

            _netCurrentHealth.onChangedWithOld -= HandleNetCurrentHealthChanged;

            _context = null;

            _healthModule = null;
        }

        [ServerRpc]
        private void SetHealthRpc(int health)
        {
            _netCurrentHealth.value = health;
        }

        private void HandleNetCurrentHealthChanged(int previous, int current)
        {
            _healthModule.RaiseChanged(previous, current);
        }
    }
}