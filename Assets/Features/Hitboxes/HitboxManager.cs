using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Pools;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Hitboxes
{
    public interface IHitboxManagerListener
    { }

    public class HitboxManager : GameSystem<IHitboxManagerListener>
    {
        private PoolManager _poolManager;

        private HitboxManagerConfig _config;

        public HitboxManagerConfig Config => _config;

        public override void Initialise(GameManagerConfig config)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _config = config.HitboxManagerConfig;

            base.Initialise(config);
        }

        public void SpawnHitbox(HitboxData data, SpawnParams parameters)
        {
            Hitbox hitbox = _poolManager.GetTypedPoolable<Hitbox>(parameters);
            hitbox.Initialise(data);
        }
    }
}