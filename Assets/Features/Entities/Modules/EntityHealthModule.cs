using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.UI;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace NoMoreFishAndChips.Entities
{
    public class EntityHealthModule
    {
        private PoolManager _poolManager;

        private IEntity _entity;
        private Func<int> _healthGetter;
        private Action<int> _healthSetter;

        private int _max;
        public int Max => _max;

        public int Current => _healthGetter();

        public event Action<int, int> OnChanged;

        /// <param name="healthSetter">Does not require clamping - we do that for you</param>
        public EntityHealthModule(IEntity entity, Func<int> healthGetter, Action<int> healthSetter)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _entity = entity;
            _healthGetter = healthGetter;
            _healthSetter = healthSetter;

            _max = _entity.EntityDefinitionData.Health;
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

            _healthSetter(health);
        }

        public void ChangeHealth(int change)
        {
            SetHealth(Current + change);
        }

        public void HandleChanged(int previous, int current)
        {
            if (current < previous)
            {
                FloatingText text = _poolManager.GetTypedPoolable<FloatingText>(new SpawnParams() { Position = _entity.transform.position + Vector3.up });
                int difference = Mathf.Abs(current - previous);
                text.Setup(difference.ToString());
            }
            
            OnChanged?.Invoke(previous, current);
        }
    }
}