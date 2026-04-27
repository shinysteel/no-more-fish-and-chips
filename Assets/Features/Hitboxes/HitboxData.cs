using FishFlingers.Entities;
using FishFlingers.Hitboxes;
using System;
using System.Linq;
using UnityEngine;

namespace FishFlingers.Hitboxes
{
    [CreateAssetMenu(fileName = "HitboxData", menuName = "Data/Hitboxes/HitboxData")]
    public class HitboxData : ScriptableObject
    {
        [SerializeField] private int _damage = 1;
        [SerializeField] private float _knockbackForceStrength = 1f;
        [SerializeField] private float _knockbackTorqueStrength = 0.5f;
        [SerializeField] private float _stunDuration = 0.2f;
        [SerializeField] private EntityAlliance _alliance = EntityAlliance.Ally;
        [SerializeField] private HitboxStep[] _steps = new HitboxStep[0];

        public int Damage => _damage;
        public float KnockbackForceStrength => _knockbackForceStrength;
        public float KnockbackTorqueStrength => _knockbackTorqueStrength;
        public float StunDuration => _stunDuration;
        public EntityAlliance Alliance => _alliance;
        public HitboxStep[] Steps => _steps;

        // We don't cache this since it's nice to have it update in realtime while editing
        public float HitboxDuration => _steps.Max(step => step.StartTime + step.Duration);
    }

    [Serializable]
    public class HitboxStep
    {
        [SerializeField] private HitboxShape _shape = HitboxShape.Box;
        [SerializeField] private Vector3 _offset = Vector3.zero;
        [SerializeField] private Vector3 _size = Vector3.one;
        [SerializeField] private float _radius = 1f;
        [SerializeField] private float _startTime = 0f;
        [SerializeField] private float _duration = 0.5f;

        public HitboxShape Shape => _shape;
        public Vector3 Offset => _offset;
        public Vector3 Size => _size;
        public float Radius => _radius;
        public float StartTime => _startTime;
        public float Duration => _duration;

        public bool InTimeWindow(float time)
        {
            return time >= _startTime && time < _startTime + _duration;
        }

        public Vector3 GetPosition(Transform hitboxTransform)
        {
            return hitboxTransform.TransformPoint(_offset);
        }
    }
}