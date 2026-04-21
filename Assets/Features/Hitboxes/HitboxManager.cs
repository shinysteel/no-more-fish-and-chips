using FishFlingers.Entities;
using FishFlingers.Pools;
using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Hitboxes
{
    public interface IHitboxManagerListener
    { }

    public class HitboxManager : GameSystem<IHitboxManagerListener>
    {
        private HitboxManagerConfig _config;

        public HitboxManagerConfig Config => _config;

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.HitboxManagerConfig;

            base.Initialise(config);
        }
    }
}