using System;
using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "CharacterPhysicsSettings", menuName = "Settings/Entities/CharacterPhysicsSettings")]
    public class CharacterPhysicsSettings : ScriptableObject
    {
        [SerializeField] private CharacterGroundDetectionSettings _groundDetection;

        public CharacterGroundDetectionSettings GroundDetection => _groundDetection;
    }

    [Serializable]
    public class CharacterGroundDetectionSettings
    {
        [SerializeField] private LayerMask _mask;
        [SerializeField] private float _castRadius = 0.125f;
        [SerializeField] private float _castDist = 0.05f;

        public LayerMask Mask => _mask;
        public float CastRadius => _castRadius;
        public float CastDist => _castDist;
    }
}