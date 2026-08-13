using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Pools;
using NUnit.Framework;
using ShinyOwl.Common;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Hitboxes
{
    public interface IHitboxManagerListener
    {
        void OnHitboxesChanged(IReadOnlyList<Hitbox> hitboxes);
    }

    public class HitboxManager : GameSystem<IHitboxManagerListener>, IPoolManagerListener
    {
        private PoolManager _poolManager;

        private HitboxManagerConfig _config;

        public HitboxManagerConfig Config => _config;

        private List<Hitbox> _hitboxes = new();
        public IReadOnlyList<Hitbox> Hitboxes => _hitboxes;
        
        public override void InitialiseConfig(GameManagerConfig config)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _poolManager.AddListener(this);

            _config = config.HitboxManagerConfig;

            base.InitialiseConfig(config);
        }

        public override void Dispose()
        {
            _poolManager?.RemoveListener(this);

            base.Dispose();
        }

        public void SpawnHitbox(HitboxData data, SpawnParams parameters)
        {
            Hitbox hitbox = _poolManager.GetTypedPoolable<Hitbox>(parameters);
            hitbox.Initialise(data);
            _hitboxes.Add(hitbox);
            NotifyHitboxesChanged();
        }

        void IPoolManagerListener.OnPoolableReturned(IPoolable poolable)
        {
            if (poolable is Hitbox hitbox)
            {
                _hitboxes.Remove(hitbox);
                NotifyHitboxesChanged();
            }
        }

        private void NotifyHitboxesChanged()
        {
            Listeners.Dispatch(listener => listener.OnHitboxesChanged(_hitboxes));
        }
    }
}