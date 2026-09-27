using NoMoreFishAndChips.Hitboxes;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "TeslaCoilDefinitionData", menuName = "Data/Entities/Structures/TeslaCoilDefinitionData")]
    public class TeslaCoilDefinitionData : StructureDefinitionData
    {
        [SerializeField] private Vector3 _electricityOffset = Vector3.up * 0.75f;
        [SerializeField] private ChargeSettings _chargeSettings;
        [SerializeField] private ElectrocuteSettings _electrocuteSettings;

        public Vector3 ElectricityOffset => _electricityOffset;
        public ChargeSettings ChargeSettings => _chargeSettings;
        public ElectrocuteSettings ElectrocuteSettings => _electrocuteSettings;
    }

    [Serializable]
    public class ChargeSettings
    {
        [SerializeField] private float _duration = 10f;
        [SerializeField] private float[] _thresholds = new float[0];
        [SerializeField] private float _pitchMultiplier = 0.25f;

        public float Duration => _duration;
        public float[] Thresholds => _thresholds;
        public float PitchMultiplier => _pitchMultiplier;
    }

    [Serializable]
    public class ElectrocuteSettings
    {
        [SerializeField] private float _radius = 2f;
        [SerializeField] private LayerMask _mask;
        [SerializeField] private Hit _hit = new();
        [SerializeField] private Texture2D[] _textures = new Texture2D[0];
        [SerializeField] private Vector3 _offset = Vector3.up;
        [SerializeField] private float _frequency = 2f;

        public float Radius => _radius;
        public LayerMask Mask => _mask;
        public Hit Hit => _hit;
        public Texture2D[] Textures => _textures;
        public Vector3 Offset => _offset;
        public float Frequency => _frequency;
    }
}