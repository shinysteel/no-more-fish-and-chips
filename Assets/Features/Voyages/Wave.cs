using NoMoreFishAndChips.States;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Voyages
{
    [Serializable]
    public class Wave
    {
        [SerializeField] private WaveStep[] _steps;

        public WaveStep[] Steps => _steps;
    }
}