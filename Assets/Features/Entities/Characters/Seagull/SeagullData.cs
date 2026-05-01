using System;
using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "SeagullData", menuName = "Data/Entities/Characters/SeagullData")]
    public class SeagullData : CharacterData
    {
        [SerializeField] private SeagullFlySettings _flySettings;

        public SeagullFlySettings FlySettings => _flySettings;
    }


    [Serializable]
    public class SeagullFlySettings
    {
        [SerializeField] private float _speed = 2.5f;
        [SerializeField] private float _acceleration = 2.5f;

        public float Speed => _speed;
        public float Acceleration => _acceleration;
    }
}