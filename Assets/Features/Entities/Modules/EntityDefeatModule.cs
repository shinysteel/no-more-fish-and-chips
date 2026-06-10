using NoMoreFishAndChips.Items;
using PrimeTween;
using UnityEngine;
using System;
using ShinyOwl.Common;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.Environments;

namespace NoMoreFishAndChips.Entities
{
    /// <summary>
    /// The role of a module that includes a getter and setter is such that it is the source of truth for its area.
    /// In this case, a defeat module should be used to set the defeat status of an entity, as well as retrieve that
    /// for all clients
    /// </summary>
    public class EntityDefeatModule
    {
        protected EntityManager _entityManager;
        protected ItemManager _itemManager;
        protected NetworkManager _networkManager;
        protected PoolManager _poolManager;
        protected EnvironmentManager _environmentManager;

        private IEntity _entity;
        private Func<bool> _isDefeatedGetter;
        protected Action<bool> _isDefeatedSetter;

        private EntityDefeatSettings _settings;

        public bool IsDefeated => _isDefeatedGetter();

        public event Action<bool> OnIsDefeatedChanged;

        public EntityDefeatModule(IEntity entity, Func<bool> isDefeatedGetter, Action<bool> isDefeatedSetter)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _itemManager = GameManager.Instance.Get<ItemManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();
            _environmentManager = GameManager.Instance.Get<EnvironmentManager>();

            _entity = entity;
            _isDefeatedGetter = isDefeatedGetter;
            _isDefeatedSetter = isDefeatedSetter;

            _settings = _entity.EntityDefinitionData.EntityDefeatSettings;

            _entity.EntityHealthModule.OnChanged += HandleHealthChanged;
        }

        ~EntityDefeatModule()
        {
            if (_entity != null)
            {
                _entity.EntityHealthModule.OnChanged -= HandleHealthChanged;
            }
        }

        public virtual void Tick()
        { }

        public virtual void FixedTick()
        { }

        private void HandleHealthChanged(int previous, int current)
        {
            if (!_entity.isOwner)
            {
                return;
            }

            if (IsDefeated)
            {
                return;
            }

            if (current > 0)
            {
                return;
            }

            SetIsDefeated(true);
        }

        public void SetIsDefeated(bool defeated)
        {
            if (IsDefeated == defeated)
            {
                return;
            }
            
            _isDefeatedSetter(defeated);
        }

        // 'Handle' can be misleading, but really this is just listening to the output of the setter, which CAN be async. This then needs to be
        // broadcasted to other listeners
        public virtual void HandleIsDefeatedChanged(bool defeated)
        {
            RaiseIsDefeatedChanged();

            // Entities are local, so they need to be 'despawned' on all clients. Immediate despawn when defeated is standard, but can be overridden
            if (IsDefeated)
            {
                Despawn();
            }
        }

        protected virtual void Despawn()
        {
            if (_networkManager.IsServer)
            {
                _itemManager.SpawnDrops(_entity.transform.position, DroppedItemType.Default, _entity.EntityDefinitionData.DropTables);
            }

            _entityManager.Despawn(_entity);
        }

        protected void RaiseIsDefeatedChanged()
        {
            OnIsDefeatedChanged?.Invoke(IsDefeated);
        }
    }
}