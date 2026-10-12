using NoMoreFishAndChips.Hitboxes;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "TentacleDefinitionData", menuName = "Data/Entities/Characters/TentacleDefinitionData")]
    public class TentacleDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private TentacleArriveSettings _arriveSettings;
        [SerializeField] private TentacleApproachSettings _approachSettings;
        [SerializeField] private TentacleSlamSettings _slamSettings;

        public TentacleArriveSettings ArriveSettings => _arriveSettings;
        public TentacleApproachSettings ApproachSettings => _approachSettings;
        public TentacleSlamSettings SlamSettings => _slamSettings;
    }

    [Serializable]
    public class TentacleArriveSettings
    {
        [SerializeField] private float _depth = 2f;
        [SerializeField] private float _duration = 2f;

        public float Depth => _depth;
        public float Duration => _duration;
    }

    [Serializable]
    public class TentacleApproachSettings
    {
        [SerializeField] private LayerMask _tileMask;
        [SerializeField] private float _moveSpeed = 1f;
        [SerializeField] private float _moveAcceleration = 10f;
        [SerializeField] private float _rotateSpeed = 7.5f;

        public LayerMask TileMask => _tileMask;
        public float MoveSpeed => _moveSpeed;
        public float MoveAcceleration => _moveAcceleration;
        public float RotateSpeed => _rotateSpeed;
    }

    [Serializable]
    public class TentacleSlamSettings
    {
        [SerializeField] private float _rotateSpeed = 7.5f;
        [SerializeField] private float _chargeDuration = 3f;
        [SerializeField] private HitboxData _hitboxData;

        public float RotateSpeed => _rotateSpeed;
        public float ChargeDuration => _chargeDuration;
        public HitboxData HitboxData => _hitboxData;
    }
}