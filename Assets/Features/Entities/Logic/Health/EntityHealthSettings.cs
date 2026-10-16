using UnityEngine;
using System;

namespace NoMoreFishAndChips.Entities
{
    [Serializable]
    public class EntityHealthSettings
    {
        [SerializeField] private int _health = 1;
        [SerializeField] private bool _isDamageable = true;

        public int Health => _health;
        public bool IsDamageable => _isDamageable;
    }
}