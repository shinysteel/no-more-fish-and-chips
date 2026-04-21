using System;
using UnityEngine;
using UnityEngine.Events;

namespace FishFlingers.Entities
{
    public class EntityHealthModule
    {
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
            _entity = entity;
            _getter = getter;
            _setter = setter;

            _max = _entity.EntityData.Health;
        }

        public void SetHealth(int health)
        {
            health = Mathf.Clamp(health, 0, _max);

            if (Current == health)
            {
                return;
            }

            int previous = Current;

            _setter(health);

            OnChanged?.Invoke(previous, Current);
        }

        public void ChangeHealth(int change)
        {
            SetHealth(Current + change);
        }
    }
}