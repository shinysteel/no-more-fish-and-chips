using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "DrowningDefinitionData", menuName = "Data/Entities/Characters/DrowningDefinitionData")]
    public class DrowningDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private DrowningChaseSettings _chaseSettings;

        public DrowningChaseSettings ChaseSettings => _chaseSettings;
    }

    [Serializable]
    public class DrowningChaseSettings
    {
        [SerializeField] private float _speedGrowth = 1.33f;

        public float SpeedGrowth => _speedGrowth;
    }
}