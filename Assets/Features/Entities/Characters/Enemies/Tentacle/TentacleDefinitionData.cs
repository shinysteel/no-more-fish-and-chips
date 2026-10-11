using NoMoreFishAndChips.Hitboxes;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "TentacleDefinitionData", menuName = "Data/Entities/Characters/TentacleDefinitionData")]
    public class TentacleDefinitionData : CharacterDefinitionData
    {
        [Serializable]
        private class TentacleArriveSettings
        { }

        [Serializable]
        private class TentacleSlamSettings
        {
            [SerializeField] private HitboxData _hitboxData;

            public HitboxData HitboxData => _hitboxData;
        }

        [Serializable]
        private class TentacleApproachSettings
        { }
    }
}