using System;
using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerPhysicsSettings", menuName = "Settings/Entities/RaftPlayerPhysicsSettings")]
    public class RaftPlayerPhysicsSettings : CharacterPhysicsSettings
    {
        [SerializeField] private RaftPlayerMoveSettings _move;
        [SerializeField] private RaftPlayerLookSettings _look;
        [SerializeField] private RaftPlayerJumpSettings _jump;
        [SerializeField] private RaftPlayerSwimSettings _swim;

        public RaftPlayerMoveSettings Move => _move;
        public RaftPlayerLookSettings Look => _look;
        public RaftPlayerJumpSettings Jump => _jump;
        public RaftPlayerSwimSettings Swim => _swim;
    }

    [Serializable]
    public class RaftPlayerMoveSettings
    {
        [SerializeField] private float _speed = 2f;
        [SerializeField] private float _acceleration = 10f;
        [SerializeField] private float _deceleration = 7.5f;
        [SerializeField] private float _attackWindupMultiplier = 0.25f;
        [SerializeField] private float _attackImpactMultiplier = 0.1f;

        public float Speed => _speed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float AttackWindupMultiplier => _attackWindupMultiplier;
        public float AttackImpactMultiplier => _attackImpactMultiplier;
    }

    [Serializable]
    public class RaftPlayerLookSettings
    {
        [SerializeField] private float _speed = 7.5f;
        [SerializeField] private float _attackImpactMultiplier = 0.25f;

        public float Speed => _speed;
        public float AttackImpactMultiplier => _attackImpactMultiplier;
    }

    [Serializable]
    public class RaftPlayerJumpSettings
    {
        [SerializeField] private float _strength = 4f;
        [SerializeField] private float _cooldown = 0.1f;

        public float Strength => _strength;
        public float Cooldown => _cooldown;
    }

    [Serializable]
    public class RaftPlayerSwimSettings
    {
        [SerializeField] private LayerMask _mask;
        [SerializeField] private float _ascendStrength = 30f;
        [SerializeField] private float _ascendDepthThreshold = 0.25f;

        public LayerMask Mask => _mask;
        public float AscendStrength => _ascendStrength;
        public float AscendDepthThreshold => _ascendDepthThreshold;
    }
}