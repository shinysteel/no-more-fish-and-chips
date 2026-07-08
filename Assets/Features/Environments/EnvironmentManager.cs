using NoMoreFishAndChips.Pools;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public interface IEnvironmentManagerListener
    { }

    public class EnvironmentManager : GameSystem<IEnvironmentManagerListener>
    {
        private PoolManager _poolManager;

        private EnvironmentManagerConfig _config;

        private Dictionary<PropId, Prop> _idPrefabMap = new();
        private Dictionary<PropId, Pool<Prop>> _propPools = new();

        public override void InitialiseConfig(GameManagerConfig config)
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _config = config.EnvironmentManagerConfig;

            foreach (Prop prop in _config.PropScanner.GetAssets())
            {
                if (prop.Id == PropId.None)
                {
                    continue;
                }

                _idPrefabMap.Add(prop.Id, prop);
            }

            base.InitialiseConfig(config);
        }

        public Prop GetProp(PropId id, SpawnParams parameters)
        {
            return _poolManager.GetPoolable(_propPools, id, _idPrefabMap[id], parameters);
        }

        public void ReturnProp(Prop prop)
        {
            _poolManager.ReturnPoolable(prop, prop.Id, _propPools);
        }
    }
}