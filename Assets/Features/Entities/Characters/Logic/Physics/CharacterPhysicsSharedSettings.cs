using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "CharacterPhysicsSharedSettings", menuName = "Settings/Entities/Characters/CharacterPhysicsSharedSettings")]
    public class CharacterPhysicsSharedSettings : EntityPhysicsSharedSettings
    {
        [SerializeField] private CharacterContactDetectionSettings _contactDetection;

        public CharacterContactDetectionSettings ContactDetection => _contactDetection;
    }

    [Serializable]
    public class CharacterContactDetectionSettings
    {
        [SerializeField] private float _groundCastRadius = 0.125f;
        [SerializeField] private float _groundCastDistance = 0.05f;
        [SerializeField] private LayerMask _groundMask;

        public float GroundCastRadius => _groundCastRadius;
        public float GroundCastDistance => _groundCastDistance;
        public LayerMask GroundMask => _groundMask;
    }
}