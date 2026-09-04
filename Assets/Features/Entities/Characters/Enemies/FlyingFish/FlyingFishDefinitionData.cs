using NoMoreFishAndChips.Hitboxes;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "FlyingFishDefinitionData", menuName = "Data/Entities/Characters/FlyingFishDefinitionData")]
    public class FlyingFishDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private FlyingFishFlySettings _flySettings;

        public FlyingFishFlySettings FlySettings => _flySettings;
    }

    [Serializable]
    public class FlyingFishFlySettings
    {
        [SerializeField] private LayerMask _mask;
        [SerializeField] private HitboxData _hitboxData;

        public LayerMask Mask => _mask;
        public HitboxData HitboxData => _hitboxData;
    }
}