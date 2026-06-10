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
        private Func<int> _getter;
        private Action<int> _setter;

        private int _max;
        public int Max => _max;

        public int Current => _getter();

        public event Action<int, int> OnChanged;

        /// <param name="setter">Does not require clamping - we do that for you</param>
        public EntityHealthModule(IEntity entity, Func<int> getter, Action<int> setter)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _entity = entity;
            _getter = getter;
            _setter = setter;

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

            _setter(health);
        }

        public void ChangeHealth(int change)
        {
            SetHealth(Current + change);
        }

        public void HandleChanged(int previous, int current)
        {
            FloatingText text = _poolManager.GetTypedPoolable<FloatingText>(new SpawnParams() { Position = _entity.transform.position + Vector3.up });
            int difference = Mathf.Abs(current - previous);
            text.Setup(difference.ToString());
            
            OnChanged?.Invoke(previous, current);
        }
    }
}