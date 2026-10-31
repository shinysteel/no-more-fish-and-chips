using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "TreeDefinitionData", menuName = "Data/Entities/TreeDefinitionData")]
    public class TreeDefinitionData : EntityDefinitionData
    {
        [SerializeField] private float _growthDuration = 60f;
        [SerializeField] private float _swayRoll = 10f;

        public float GrowthDuration => _growthDuration;
        public float SwayRoll => _swayRoll;
    }
}