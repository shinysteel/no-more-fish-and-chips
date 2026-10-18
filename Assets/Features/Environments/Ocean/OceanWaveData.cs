using UnityEngine;
using System;

namespace NoMoreFishAndChips.Environments
{
    [Serializable]
    public class OceanWaveData
    {
        [SerializeField] private Vector2 _direction = Vector2.down;
        [SerializeField] private float _length = 2f;
        [SerializeField] private float _amplitude = 0.025f;
        [SerializeField] private float _speed = 0.5f;
        [SerializeField] private float _steepness = 8f;

        public Vector2 Direction => _direction;
        public float Length => _length;
        public float Amplitude => _amplitude;
        public float Speed => _speed;
        public float Steepness => _steepness;
    }
}