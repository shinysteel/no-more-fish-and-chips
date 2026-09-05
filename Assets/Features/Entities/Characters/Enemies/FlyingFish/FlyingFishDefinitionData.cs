using NoMoreFishAndChips.Hitboxes;
using ShinyOwl.Common;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "FlyingFishDefinitionData", menuName = "Data/Entities/Characters/FlyingFishDefinitionData")]
    public class FlyingFishDefinitionData : CharacterDefinitionData
    {
        [SerializeField] private FlyingFishSurfaceSettings _surfaceSettings;
        [SerializeField] private FlyingFishFlySettings _flySettings;

        public FlyingFishSurfaceSettings SurfaceSettings => _surfaceSettings;
        public FlyingFishFlySettings FlySettings => _flySettings;
    }

    [Serializable]
    public class FlyingFishSurfaceSettings
    {
        [SerializeField] private IntRange _offsetRange = new IntRange(2, 4);
        [SerializeField] private float _depth = 0.5f;
        [SerializeField] private float _wiggleDuration = 1f;
        [SerializeField] private float _wigglePitch = -15f;
        [SerializeField] private float _pitchDuration = 0.25f;
        [SerializeField] private FloatRange _distanceRange = new FloatRange(2f, 4f);
        [SerializeField] private FloatRange _pitchRange = new FloatRange(-75f, -60f);

        public IntRange OffsetRange => _offsetRange;
        public float Depth => _depth;
        public float WiggleDuration => _wiggleDuration;
        public float WigglePitch => _wigglePitch;
        public float PitchDuration => _pitchDuration;
        public FloatRange DistanceRange => _distanceRange;
        public FloatRange PitchRange => _pitchRange;
    }

    [Serializable]
    public class FlyingFishFlySettings
    {
        [SerializeField] private float _forceStrength = 50f;
        [SerializeField] private float _rotateDuration = 1f;
        [SerializeField] private float _calculateDuration = 3f;
        [SerializeField] private Vector3 _markerScale = Vector3.one * 0.5f;
        [SerializeField] private LayerMask _markerMask;
        [SerializeField] private float _blendDistance = 2f;
        [SerializeField] private HitboxData _hitboxData;

        public float ForceStrength => _forceStrength;
        public float RotateDuration => _rotateDuration;
        public float CalculateDuration => _calculateDuration;
        public Vector3 MarkerScale => _markerScale;
        public LayerMask MarkerMask => _markerMask;
        public float BlendDistance => _blendDistance;
        public HitboxData HitboxData => _hitboxData;
    }
}