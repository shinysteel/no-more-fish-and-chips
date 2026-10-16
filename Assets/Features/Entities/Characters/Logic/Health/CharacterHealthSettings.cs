using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [Serializable]
    public class CharacterHealthSettings
    {
        [SerializeField] private float _healthBarOffset = 0.5f;

        public float HealthBarOffset => _healthBarOffset;
    }
}