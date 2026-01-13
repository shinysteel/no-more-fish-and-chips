using System;
using UnityEngine;

namespace FishFlingers.Entities
{
    [Serializable]
    public class MoveSettings
    {
        [SerializeField] private float _speed = 2f;
        [SerializeField] private float _acceleration = 10f;
        [SerializeField] private float _deceleration = 7.5f;

        public float Speed => _speed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
    }

    [Serializable]
    public class JumpSettings
    {
        [SerializeField] private float _strength = 4f;
        [SerializeField] private float _cooldown = 0.1f;

        public float Strength => _strength;
        public float Cooldown => _cooldown;
    }

    [Serializable]
    public class GroundDetectionSettings
    {
        [SerializeField] private LayerMask _mask;
        [SerializeField] private float _castRadius = 0.125f;
        [SerializeField] private float _castDist = 0.05f;

        public LayerMask Mask => _mask;
        public float CastRadius => _castRadius;
        public float CastDist => _castDist;
    }

    [Serializable]
    public class SwimSettings
    {
        [SerializeField] private LayerMask _mask;
        [SerializeField] private float _ascendStrength = 30f;
        [SerializeField] private float _ascendDepthThreshold = 0.25f;

        public LayerMask Mask => _mask;
        public float AscendStrength => _ascendStrength;
        public float AscendDepthThreshold => _ascendDepthThreshold;
    }

    [CreateAssetMenu(fileName = "RaftPlayerData", menuName = "Data/Entities/Characters/RaftPlayerData")]
    public class RaftPlayerData : CharacterData
    {
        [SerializeField] private MoveSettings _moveSettings;
        [SerializeField] private JumpSettings _jumpSettings;
        [SerializeField] private GroundDetectionSettings _groundDetectionSettings;
        [SerializeField] private SwimSettings _swimSettings;

        public MoveSettings MoveSettings => _moveSettings;
        public JumpSettings JumpSettings => _jumpSettings;
        public GroundDetectionSettings GroundDetectionSettings => _groundDetectionSettings;
        public SwimSettings SwimSettings => _swimSettings;
    }
}