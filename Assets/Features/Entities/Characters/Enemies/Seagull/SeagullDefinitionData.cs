using NoMoreFishAndChips.Hitboxes;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "SeagullDefinitionData", menuName = "Data/Entities/Characters/SeagullDefinitionData")]
    public class SeagullDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private SeagullFlySettings _flySettings;
        [SerializeField] private SeagullAttackSettings _attackSettings;

        public SeagullFlySettings FlySettings => _flySettings;
        public SeagullAttackSettings AttackSettings => _attackSettings;
    }


    [Serializable]
    public class SeagullFlySettings
    {
        [SerializeField] private float _speed = 2.5f;
        [SerializeField] private float _acceleration = 2.5f;

        public float Speed => _speed;
        public float Acceleration => _acceleration;
    }

    [Serializable]
    public class SeagullAttackSettings
    {
        [SerializeField] private float _range = 0.5f;
        [SerializeField] private LayerMask _mask;
        [SerializeField] private HitboxData _hitboxData;

        public float Range => _range;
        public LayerMask Mask => _mask;
        public HitboxData HitboxData => _hitboxData;
    }
}