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
        WaterSplash,
        TeslaElectricity
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
            effectManager.GetVfx(id, position, null);
        }

        public VFX GetVfx(VfxId id, Vector3 position, Transform parent)
        {
            return _poolManager.GetPoolable(_vfxPools, id, _vfxIdPrefabMap[id], new SpawnParams() { Position = position, Parent = parent });
        }

        public void ReturnVfx(VFX vfx)
        {
            _poolManager.ReturnPoolable(vfx, vfx.VfxId, _vfxPools);
        }
    }
}