using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ShinyOwl.Common
{
    [Serializable]
    public class WeightedEntry<T>
    {
        [SerializeField] private T _value;
        [SerializeField] private float _weight = 1f;

        public T Value => _value;
        public float Weight => _weight;

        public WeightedEntry(T value, float weight)
        {
            _value = value;
            _weight = weight;
        }
    }

    public class WeightedPicker<T>
    {
        private List<WeightedEntry<T>> _entries = new();

        public void Set(List<WeightedEntry<T>> entries)
        {
            Clear();
            _entries.AddRange(entries);
        }

        public void Clear()
        {
            _entries.Clear();
        }
        
        public T Pick()
        {
            float total = 0f;

            foreach (WeightedEntry<T> item in _entries)
            {
                total += item.Weight;
            }

            float random = Random.Range(0f, total);
            float cumulative = 0f;

            foreach (WeightedEntry<T> item in _entries)
            {
                cumulative += item.Weight;
                if (random < cumulative)
                {
                    return item.Value;
                }
            }

            return default;
        }
    }
}