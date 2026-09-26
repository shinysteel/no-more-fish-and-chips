using UnityEngine;
using System;

namespace NoMoreFishAndChips.Hitboxes
{
    [Serializable]
    public class Hit
    {
        [SerializeField] private int _healthDamage = 1;
        [SerializeField] private float _poiseDamage = 0.2f;
        [SerializeField] private float _forceStrength = 1f;
        [SerializeField] private float _torqueStrength = 0.5f;

        public int HealthDamage => _healthDamage;
        public float PoiseDamage => _poiseDamage;
        public float ForceStrength => _forceStrength;
        public float TorqueStrength => _torqueStrength;
    }
}