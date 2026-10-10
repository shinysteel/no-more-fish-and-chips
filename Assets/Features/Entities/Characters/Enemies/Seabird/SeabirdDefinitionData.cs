using NoMoreFishAndChips.Hitboxes;
using ShinyOwl.Common.Structures;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "SeabirdDefinitionData", menuName = "Data/Entities/Characters/SeabirdDefinitionData")]
    public class SeabirdDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private float _glideDistance = 2f;
        [SerializeField] private LayerMask _glideMask;
        [SerializeField] private SeabirdArriveSettings _arriveSettings;
        [SerializeField] private SeabirdAirSettings _airSettings;
        [SerializeField] private SeabirdGroundSettings _groundSettings;
        [SerializeField] private SeabirdWaterSettings _waterSettings;

        public float GlideDistance => _glideDistance;
        public LayerMask GlideMask => _glideMask;
        public SeabirdArriveSettings ArriveSettings => _arriveSettings;
        public SeabirdAirSettings AirSettings => _airSettings;
        public SeabirdGroundSettings GroundSettings => _groundSettings;
        public SeabirdWaterSettings WaterSettings => _waterSettings;
    }

    [Serializable]
    public class SeabirdArriveSettings
    {
        [SerializeField] private float _delay = 0.5f;
        [SerializeField] private float _startAltitude = 5f;
        [SerializeField] private float _dampingStrength = 2f;
        [SerializeField] private float _duration = 1f;

        public float Delay => _delay;
        public float StartAltitude => _startAltitude;
        public float DampingStrength => _dampingStrength;
        public float Duration => _duration;
    }

    [Serializable]

    public class SeabirdAirSettings
    {
        [SerializeField] private SeabirdAirTakeoffSettings _takeoffSettings;
        [SerializeField] private SeabirdAirStrafeSettings _strafeSettings;
        [SerializeField] private SeabirdAirLandSettings _landSettings;

        public SeabirdAirTakeoffSettings TakeoffSettings => _takeoffSettings;
        public SeabirdAirStrafeSettings StrafeSettings => _strafeSettings;
        public SeabirdAirLandSettings LandSettings => _landSettings;
    }

    [Serializable]
    public class SeabirdAirTakeoffSettings
    {
        [SerializeField] private float _speed = 11f;
        [SerializeField] private float _acceleration = 11f;

        public float Speed => _speed;
        public float Acceleration => _acceleration;
    }

    [Serializable]
    public class SeabirdAirStrafeSettings
    {
        [SerializeField] private float _strafeDuration = 1f;
        [SerializeField] private float _strafeAcceleration = 2.5f;
        [SerializeField] private float _strafeRoll = 20f;
        [SerializeField] private float _rotateSpeed = 2.5f;
        [SerializeField] private float _brakeDuration = 1f;
        [SerializeField] private float _brakeStrength = 1f;
        [SerializeField] private float _dampingStrength = 2f;
        
        public float StrafeDuration => _strafeDuration;
        public float StrafeAcceleration => _strafeAcceleration;
        public float StrafeRoll => _strafeRoll;
        public float RotateSpeed => _rotateSpeed;
        public float BrakeDuration => _brakeDuration;
        public float BrakeStrength => _brakeStrength;
        public float DampingStrength => _dampingStrength;
    }

    [Serializable]
    public class SeabirdAirLandSettings
    {
        [SerializeField] private float _alignAcceleration = 2f;
        [SerializeField] private float _alignDeceleration = 2f;
        [SerializeField] private float _rotateSpeed = 5f;
        [SerializeField] private float _flapStrength = 5f;

        public float AlignAcceleration => _alignAcceleration;
        public float AlignDeceleration => _alignDeceleration;
        public float RotateSpeed => _rotateSpeed;
        public float FlapStrength => _flapStrength;
    }

    [Serializable]
    public class SeabirdGroundSettings
    {
        [SerializeField] private float _squawkRange = 0.5f;
        [SerializeField] private LayerMask _squawkMask;
        [SerializeField] private SeabirdGroundIdleSettings _idleSettings;
        [SerializeField] private SeabirdGroundRoamSettings _roamSettings;
        [SerializeField] private SeabirdGroundSquawkSettings _squawkSettings;

        public float SquawkRange => _squawkRange;
        public LayerMask SquawkMask => _squawkMask;
        public SeabirdGroundIdleSettings IdleSettings => _idleSettings;
        public SeabirdGroundRoamSettings RoamSettings => _roamSettings;
        public SeabirdGroundSquawkSettings SquawkSettings => _squawkSettings;
    }

    [Serializable]
    public class SeabirdGroundIdleSettings
    {
        [SerializeField] private FloatRange _idleRange = new FloatRange(2f, 3f);

        public FloatRange IdleRange => _idleRange;
    }

    [Serializable]
    public class SeabirdGroundRoamSettings
    {
        [SerializeField] private float _moveSpeed = 1f;
        [SerializeField] private float _moveAcceleration = 10f;
        [SerializeField] private float _rotateSpeed = 7.5f;

        public float MoveSpeed => _moveSpeed;
        public float MoveAcceleration => _moveAcceleration;
        public float RotateSpeed => _rotateSpeed;
    }

    [Serializable]
    public class SeabirdGroundSquawkSettings
    {
        [SerializeField] private HitboxData _hitboxData;

        public HitboxData HitboxData => _hitboxData;
    }

    [Serializable]
    public class SeabirdWaterSettings
    {
        [SerializeField] private FloatRange _idleRange = new FloatRange(2f, 3f);

        public FloatRange IdleRange => _idleRange;
    }
}