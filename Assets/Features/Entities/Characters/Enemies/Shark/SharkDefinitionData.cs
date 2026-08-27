using NoMoreFishAndChips.Hitboxes;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "SharkDefinitionData", menuName = "Data/Entities/Characters/SharkDefinitionData")]
    public class SharkDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private HitboxData _biteHitboxData;

        public HitboxData BiteHitboxData => _biteHitboxData;
    }
}