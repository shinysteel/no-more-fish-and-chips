using System;
using UnityEngine;

using Random = UnityEngine.Random;

namespace ShinyOwl.Common
{
    [Serializable]
    public class IntRange
    {
        [SerializeField] private int _min;
        [SerializeField] private int _max;

        public int Min => _min;
        public int Max => _max;

        public IntRange(int min, int max)
        {
            _min = min;
            _max = max;
        }

        public int RandomRange()
        {
            return Random.Range(_min, _max);
        }
    }
}