using System;
using UnityEngine;
using UnityEngine.Events;

namespace FishFlingers.Entities
{
    public class HealthModule
    {
        private Func<int> _getter;
        private Action<int> _setter;

        private Action<int, int> _onChanged;

        public int Max { get; private set; }

        public int Current => _getter();

        /// <param name="setter">Does not require clamping - we do that for you</param>
        public HealthModule(int max, Func<int> getter, Action<int> setter, Action<int, int> onChanged)
        {
            Max = max;

            _getter = getter;
            _setter = setter;

            _onChanged = onChanged;
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

            _onChanged?.Invoke(previous, Current);
        }
    }
}