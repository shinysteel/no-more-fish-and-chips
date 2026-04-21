using System;
using UnityEngine;
using UnityEngine.Events;

namespace FishFlingers.Entities
{
    public class EntityHealthModule
    {
        private int _max;
        private Func<int> _getter;
        private Action<int> _setter;

        public int Max => _max;
        public int Current => _getter();

        public event Action<int, int> OnChanged;

        /// <param name="setter">Does not require clamping - we do that for you</param>
        public EntityHealthModule(int max, Func<int> getter, Action<int> setter)
        {
            _max = max;
            _getter = getter;
            _setter = setter;
        }

        public void SetHealth(int health)
        {
            health = Mathf.Clamp(health, 0, Max);

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