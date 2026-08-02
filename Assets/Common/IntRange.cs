using System;
using UnityEngine;

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

        public void SetRange(int min, int max)
        {
            _min = min;
            _max = max;
        }
    }
}