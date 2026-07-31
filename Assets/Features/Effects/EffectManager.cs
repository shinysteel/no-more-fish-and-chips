using NoMoreFishAndChips.Environments;
using PurrNet;
using System.Collections.Generic;
using UnityEngine;
using NoMoreFishAndChips.Pools;

namespace NoMoreFishAndChips.Effects
{
    public interface IEffectManagerListener
    { }

    public enum VfxId
    {
        None,
        WaterSplash
    }

    public class EffectManager : GameSystem<IEffectManagerListener>
    {
        private PoolManager _poolManager;

        private EffectManagerConfig _config;

        private Dictionary<VfxId, VFX> _vfxIdPrefabMap = new();
        private Dictionary<VfxId, Pool<VFX>> _vfxPools = new();

        public override void InitialiseConfig(GameManagerConfig config)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _config = config.EffectManagerConfig;

            foreach (VFX vfx in _config.VfxScanner.GetAssets())
            {
                _vfxIdPrefabMap.Add(vfx.VfxId, vfx);
            }

            base.InitialiseConfig(config);
        }

        [ObserversRpc]
        public static void SpawnVfxRpc(VfxId id, Vector3 position)
        {
            EffectManager effectManager = GameManager.Instance.Get<EffectManager>();
            effectManager._poolManager.GetPoolable(effectManager._vfxPools, id, effectManager._vfxIdPrefabMap[id], new SpawnParams() { Position = position });
        }

        public void ReturnVfx(VFX vfx)
        {
            _poolManager.ReturnPoolable(vfx, vfx.VfxId, _vfxPools);
        }
    }
}