using System;
using UnityEngine;

using EntityId = NoMoreFishAndChips.Entities.EntityId;

namespace NoMoreFishAndChips.Voyages
{
    [Serializable]
    public class WaveStep
    {
        [SerializeField] private EntityId _entityId;
        [SerializeField] private int _count;
        [SerializeField] private float _interval;

        public EntityId EntityId => _entityId;
        public int Count => _count;
        public float Interval => _interval;
    }
}