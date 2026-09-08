using NoMoreFishAndChips.Hitboxes;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "SeagullDefinitionData", menuName = "Data/Entities/Characters/SeagullDefinitionData")]
    public class SeagullDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private SeagullArriveSettings _arriveSettings;
        [SerializeField] private SeagullAirSettings _airSettings;
        [SerializeField] private SeagullGroundSettings _groundSettings;
        [SerializeField] private SeagullWaterSettings _waterSettings;

        public SeagullArriveSettings ArriveSettings => _arriveSettings;
        public SeagullAirSettings AirSettings => _airSettings;
        public SeagullGroundSettings GroundSettings => _groundSettings;
        public SeagullWaterSettings WaterSettings => _waterSettings;
    }

    [Serializable]
    public class SeagullArriveSettings
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

    public class SeagullAirSettings
    {
        [SerializeField] private SeagullAirTakeoffSettings _takeoffSettings;
        [SerializeField] private SeagullAirStrafeSettings _strafeSettings;
        [SerializeField] private SeagullAirLandSettings _landSettings;

        public SeagullAirTakeoffSettings TakeoffSettings => _takeoffSettings;
        public SeagullAirStrafeSettings StrafeSettings => _strafeSettings;
        public SeagullAirLandSettings LandSettings => _landSettings;
    }

    [Serializable]
    public class SeagullAirTakeoffSettings
    { }

    [Serializable]
    public class SeagullAirStrafeSettings
    {
        [SerializeField] private float _strafeDuration = 1f;
        [SerializeField] private float _strafeAcceleration = 2.5f;
        [SerializeField] private float _strafeRoll = 20f;
        [SerializeField] private float _rotateSpeed = 2.5f;
        [SerializeField] private float _brakeDuration = 1f;
        [SerializeField] private float _dampingStrength = 1f;
        
        public float StrafeDuration => _strafeDuration;
        public float StrafeAcceleration => _strafeAcceleration;
        public float StrafeRoll => _strafeRoll;
        public float RotateSpeed => _rotateSpeed;
        public float BrakeDuration => _brakeDuration;
        public float DampingStrength => _dampingStrength;
    }

    [Serializable]
    public class SeagullAirLandSettings
    { }

    [Serializable]
    public class SeagullGroundSettings
    {
        [SerializeField] private SeagullGroundIdleSettings _idleSettings;
        [SerializeField] private SeagullGroundRoamSettings _roamSettings;
        [SerializeField] private SeagullGroundAttackSettings _attackSettings;

        public SeagullGroundIdleSettings IdleSettings => _idleSettings;
        public SeagullGroundRoamSettings RoamSettings => _roamSettings;
        public SeagullGroundAttackSettings AttackSettings => _attackSettings;
    }

    [Serializable]
    public class SeagullGroundIdleSettings
    { }

    [Serializable]
    public class SeagullGroundRoamSettings
    { }

    [Serializable]
    public class SeagullGroundAttackSettings
    {
        [SerializeField] private HitboxData _hitboxData;

        public HitboxData HitboxData => _hitboxData;
    }

    [Serializable]
    public class SeagullWaterSettings
    { }
}