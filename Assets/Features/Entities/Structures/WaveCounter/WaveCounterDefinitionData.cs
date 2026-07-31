using NoMoreFishAndChips.Hitboxes;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "WaveCounterDefinitionData", menuName = "Data/Entities/Structures/WaveCounterDefinitionData")]
    public class WaveCounterDefinitionData : StructureDefinitionData
    {
        [SerializeField] private HitboxData _slamHitboxData;

        public HitboxData SlamHitboxData => _slamHitboxData;
    }
}