using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.UI;
using PurrNet;
using ShinyOwl.Common;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace NoMoreFishAndChips.Entities
{
    public class EntityHealthLogic : EntityLogic
    {
        private UIManager _uiManager;

        private SyncVar<int> _netHealth;

        private int _maxHealth;
        public int MaxHealth => _maxHealth;

        public int CurrentHealth => _netHealth.value;

        public event Action<int, int> OnChanged;

        public EntityHealthLogic(Entity entity, SyncVar<int> netHealth) : base(entity)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();

            _netHealth = netHealth;
           
            _netHealth.onChangedWithOld += HandleNetHealthChanged;

            _maxHealth = _entity.EntityDefinitionData.Health;
        }

        public override void OnSpawned()
        {
            if (_entity.isOwner && _netHealth.value == 0)
            {
                SetHealth(_maxHealth);
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
                FloatingTextUI ui = _uiManager.CreateWorldUI(_uiManager.Config.FloatingTextUIPrefab, _entity.transform.position + Vector3.up * 0.5f);
                int difference = Mathf.Abs(current - previous);
                ui.Setup(difference.ToString());
            }

            OnChanged?.Invoke(previous, current);
        }

        public void SetHealth(int health)
        {
            if (!_entity.EntityDefinitionData.IsDamageable)
            {
                return;
            }

            health = Mathf.Clamp(health, 0, _maxHealth);

            if (_netHealth.value == health)
            {
                return;
            }

            int previous = _netHealth.value;

            _entity.SetNetHealthRpc(_entity.owner.Value, health);
        }

        public void ChangeHealth(int change)
        {
            SetHealth(_netHealth.value + change);
        }
    }
}