using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.UI;
using PurrNet;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace NoMoreFishAndChips.Entities
{
    public class EntityHealthLogic : EntityLogic
    {
        private PoolManager _poolManager;

        private SyncVar<int> _netHealth;

        private int _max;
        public int Max => _max;

        public int Current => _netHealth.value;

        public event Action<int, int> OnChanged;

        public EntityHealthLogic(Entity entity, SyncVar<int> netHealth) : base(entity)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _entity = entity;
            _netHealth = netHealth;
           
            _netHealth.onChangedWithOld += HandleNetHealthChanged;

            _max = _entity.EntityDefinitionData.Health;
        }

        public override void OnSpawned()
        {
            if (_entity.isOwner)
            {
                SetHealth(_max);
            }
        }

        public override void OnDespawned()
        {
            _netHealth.onChangedWithOld -= HandleNetHealthChanged;
        }

        private void HandleNetHealthChanged(int previous, int current)
        {
            if (current < previous)
            {
                FloatingText text = _poolManager.GetTypedPoolable<FloatingText>(new SpawnParams() { Position = _entity.transform.position + Vector3.up });
                int difference = Mathf.Abs(current - previous);
                text.Setup(difference.ToString());
            }

            OnChanged?.Invoke(previous, current);
        }

        public void SetHealth(int health)
        {
            if (!_entity.EntityDefinitionData.IsDamageable)
            {
                return;
            }

            health = Mathf.Clamp(health, 0, _max);

            if (Current == health)
            {
                return;
            }

            int previous = Current;

            _netHealth.value = health;
        }

        public void ChangeHealth(int change)
        {
            SetHealth(Current + change);
        }
    }
}