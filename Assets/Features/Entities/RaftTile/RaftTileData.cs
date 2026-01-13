using System;
using UnityEngine;

namespace FishFlingers.Entities
{
    [Serializable]
    public class BobSettings
    {
        [SerializeField] private float _amplitude = 0.125f;
        [SerializeField] private float _noiseScale = 0.5f;
        [SerializeField] private float _timeScale = 0.25f;

        public float Amplitude => _amplitude;
        public float NoiseScale => _noiseScale;
        public float TimeScale => _timeScale;
    }

    [Serializable]
    public class SinkSettings
    {
        [SerializeField] private LayerMask _mask;
        [SerializeField] private float _radius = 1f;
        [SerializeField] private float _speed = 0.25f;

        public LayerMask Mask => _mask;
        public float Radius => _radius;
        public float Speed => _speed;
    }

    [CreateAssetMenu(fileName = "RaftTileData", menuName = "Data/Entities/RaftTileData")]
    public class RaftTileData : EntityData
    {
        [SerializeField] private BobSettings _bobSettings;
        [SerializeField] private SinkSettings _sinkSettings;

        public BobSettings BobSettings => _bobSettings;
        public SinkSettings SinkSettings => _sinkSettings;
    }
}