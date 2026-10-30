using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "TreeDefinitionData", menuName = "Data/Entities/TreeDefinitionData")]
    public class TreeDefinitionData : EntityDefinitionData
    {
        [SerializeField] private float _growthDuration = 60f;

        public float GrowthDuration => _growthDuration;
    }
}