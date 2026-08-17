using NoMoreFishAndChips.Hitboxes;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "TentacleDefinitionData", menuName = "Data/Entities/Characters/TentacleDefinitionData")]
    public class TentacleDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private HitboxData _slamHitboxData;

        public HitboxData SlamHitboxData => _slamHitboxData;
    }
}