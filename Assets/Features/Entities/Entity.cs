using NoMoreFishAndChips.Cameras;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using PurrNet;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    // Maybe it's not so obvious that Entity is linked to the GameplayState, but for now they aren't used in any other state
    public abstract class Entity : GameplayBehaviour, ISurface
    {
        [SerializeField] protected EntityDefinitionData _entityDefinitionData;
        [SerializeField] protected EntityModel _entityModel;
        [SerializeField] protected Rigidbody _rigidbody;
        [SerializeField] protected Collider _collider;

        private SyncVar<int> _netHealth = new SyncVar<int>(ownerAuth: true);
        private SyncVar<bool> _netIsDefeated = new SyncVar<bool>(ownerAuth: true);

        public EntityDefinitionData EntityDefinitionData => _entityDefinitionData;
        public EntityModel EntityModel => _entityModel;

        protected EntityLogicFactory _logicFactory;
        private Dictionary<Type, EntityLogic> _typeLogicMap = new();

        public EntityHealthLogic EntityHealthLogic => GetLogic<EntityHealthLogic>();
        public EntityDefeatLogic EntityDefeatLogic => GetLogic<EntityDefeatLogic>();
        public EntityLifecycleLogic EntityLifecycleLogic => GetLogic<EntityLifecycleLogic>();
        public EntityEffectsLogic EntityEffectsLogic => GetLogic<EntityEffectsLogic>();
        public EntityPhysicsLogic EntityPhysicsLogic => GetLogic<EntityPhysicsLogic>();

        SurfaceType ISurface.SurfaceType => _entityDefinitionData.SurfaceType;

        protected override void OnInitializeModules()
        {
            _logicFactory = CreateLogicFactory();

            AddLogic(typeof(EntityHealthLogic), _logicFactory.CreateHealthLogic(this, _netHealth));
            AddLogic(typeof(EntityDefeatLogic), _logicFactory.CreateDefeatLogic(this, _netIsDefeated));
            AddLogic(typeof(EntityLifecycleLogic), _logicFactory.CreateLifecycleLogic(this));
            AddLogic(typeof(EntityEffectsLogic), _logicFactory.CreateEffectsLogic(this));
            AddLogic(typeof(EntityPhysicsLogic), _logicFactory.CreatePhysicsLogic(this, _rigidbody, _collider));
        }

        protected virtual EntityLogicFactory CreateLogicFactory()
        {
            return new EntityLogicFactory();
        }

        protected void AddLogic<T>(Type type, T logic) where T : EntityLogic
        {
            _typeLogicMap.Add(type, logic);
        }

        protected T GetLogic<T>() where T : EntityLogic
        {
            return (T)_typeLogicMap[typeof(T)];
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            foreach (EntityLogic logic in _typeLogicMap.Values)
            {
                logic.OnSpawned();
            }

            _entityManager.RaiseNetEntitySpawned(this);
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            foreach (EntityLogic logic in _typeLogicMap.Values)
            {
                logic.InitialiseContext(context);
            }
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _entityManager?.RaiseNetEntityDespawned(this);

            foreach (EntityLogic logic in _typeLogicMap.Values)
            {
                logic.OnDespawned();
            }
        }

        protected virtual void Update()
        {
            if (!isFullySpawned || _context == null)
            {
                return;
            }

            foreach (EntityLogic logic in _typeLogicMap.Values)
            {
                logic.Tick();
            }
        }

        protected virtual void FixedUpdate()
        {
            if (!isFullySpawned || _context == null)
            {
                return;
            }

            foreach (EntityLogic logic in _typeLogicMap.Values)
            {
                logic.FixedTick();
            }
        }

        [ServerRpc]
        public void SetNetHealthRpc(int health)
        {
            _netHealth.value = health;
        }

        [ObserversRpc]
        public void AnimateHurtRpc()
        {
            EntityEffectsLogic.AnimateHurt();
        }

        [TargetRpc]
        public void AddForceRpc(PlayerID id, Vector3 force)
        {
            _rigidbody.AddForce(force, ForceMode.Impulse);
        }

        [TargetRpc]
        public void AddTorqueRpc(PlayerID id, Vector3 torque)
        {
            _rigidbody.AddTorque(torque, ForceMode.Impulse);
        }
    }
}