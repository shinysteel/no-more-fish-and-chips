using System;
using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "CharacterPhysicsSettings", menuName = "Settings/Entities/CharacterPhysicsSettings")]
    public class CharacterPhysicsSettings : ScriptableObject
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
        [SerializeField] private LayerMask _waterMask;

        public float GroundCastRadius => _groundCastRadius;
        public float GroundCastDistance => _groundCastDistance;
        public LayerMask GroundMask => _groundMask;
        public LayerMask WaterMask => _waterMask;
    }
}