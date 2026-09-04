using System;
using UnityEngine;

using Random = UnityEngine.Random;

namespace ShinyOwl.Common
{
    [Serializable]
    public class FloatRange
    {
        [SerializeField] private float _min;
        [SerializeField] private float _max;

        public float Min => _min;
        public float Max => _max;

        public FloatRange(float min, float max)
        {
            _min = min;
            _max = max;
        }

        public float RandomRange()
        {
            return Random.Range(_min, _max);
        }
    }
}