using NoMoreFishAndChips.Items;
using PrimeTween;
using UnityEngine;
using System;
using ShinyOwl.Common;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.Environments;
using PurrNet;

using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;

namespace NoMoreFishAndChips.Entities
{
    /// <summary>
    /// The role of a module that includes a getter and setter is such that it is the source of truth for its area.
    /// In this case, a defeat module should be used to set the defeat status of an entity, as well as retrieve that
    /// for all clients
    /// </summary>
    public class EntityDefeatLogic : EntityLogic
    {
        protected EntityManager _entityManager;
        protected ItemManager _itemManager;
        protected NetworkManager _networkManager;
        protected PoolManager _poolManager;
        protected EnvironmentManager _environmentManager;

        private EntityDefeatSettings _settings;

        protected SyncVar<bool> _netIsDefeated;

        public bool IsDefeated => _netIsDefeated.value;

        public event Action<bool> OnIsDefeatedChanged;

        public EntityDefeatLogic(Entity entity, SyncVar<bool> netIsDefeated) : base(entity)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _itemManager = GameManager.Instance.Get<ItemManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();
            _environmentManager = GameManager.Instance.Get<EnvironmentManager>();

            _entity = entity;
            _netIsDefeated = netIsDefeated;

            _settings = _entity.EntityDefinitionData.EntityDefeatSettings;

            _netIsDefeated.onChanged += HandleNetIsDefeatedChanged;
            _entity.EntityHealthLogic.OnChanged += HandleHealthChanged;
        }

        public override void OnDespawned()
        {
            _netIsDefeated.onChanged -= HandleNetIsDefeatedChanged;

            if (_entity != null)
            {
                _entity.EntityHealthLogic.OnChanged -= HandleHealthChanged;
            }
        }

        // 'Handle' can be misleading, but really this is just listening to the output of the setter, which CAN be async. This then needs to be
        // broadcasted to other listeners
        protected virtual void HandleNetIsDefeatedChanged(bool defeated)
        {
            RaiseIsDefeatedChanged();

            // Entities are local, so they need to be 'despawned' on all clients. Immediate despawn when defeated is standard, but can be overridden
            if (_netIsDefeated.value)
            {
                Despawn();
            }
        }

        private void HandleHealthChanged(int previous, int current)
        {
            if (!_entity.isOwner)
            {
                return;
            }

            if (_netIsDefeated.value)
            {
                return;
            }

            if (current > 0)
            {
                return;
            }

            SetIsDefeated(true);
        }

        protected void RaiseIsDefeatedChanged()
        {
            OnIsDefeatedChanged?.Invoke(_netIsDefeated.value);
        }

        public void SetIsDefeated(bool defeated)
        {
            _entity.SetNetIsDefeated(_entity.owner.Value, defeated);
        }

        public virtual void Despawn()
        {
            if (_networkManager.IsServer)
            {
                _itemManager.SpawnDrops(_entity.transform.position, DroppedItemType.Default, _entity.EntityDefinitionData.DropTables);
            }

            _entityManager.Despawn(_entity);
        }
    }
}