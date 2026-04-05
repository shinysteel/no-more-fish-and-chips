using FishFlingers.Environments;
using FishFlingers.Items;
using FishFlingers.States;
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

    [CreateAssetMenu(fileName = "TileData", menuName = "Data/Entities/TileData")]
    public class TileData : EntityData, IBuildable
    {
        [SerializeField] private Recipe _recipe;
        [SerializeField] private BobSettings _bobSettings;
        [SerializeField] private SinkSettings _sinkSettings;

        public EntityData EntityData => this;
        public Recipe Recipe => _recipe;
        public BobSettings BobSettings => _bobSettings;
        public SinkSettings SinkSettings => _sinkSettings;

        public bool TryBuild(GameplayContext context, RaftPlayerTarget target)
        {
            if (target.Tile != null)
            {
                return false;
            }

            context.Raft.AddNetTileRpc(target.Cell, NetTile.MaxHealth);

            return true;
        }
    }
}