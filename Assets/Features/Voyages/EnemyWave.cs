using NoMoreFishAndChips.States;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Voyages
{
    [Serializable]
    public class EnemyWave
    {
        [SerializeField] private EnemyWaveStep[] _steps;

        public EnemyWaveStep[] Steps => _steps;
    }
}