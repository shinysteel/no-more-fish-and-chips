using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Voyages
{
    public interface IVoyageManagerListener
    { }

    public class VoyageManager : GameSystem<IVoyageManagerListener>
    {
        private VoyageManagerConfig _config;

        private Dictionary<StageId, StageData> _stageIdDataMap = new();

        public override void InitialiseConfig(GameManagerConfig config)
        {
            _config = config.VoyageManagerConfig;

            foreach (StageData data in _config.StageDataScanner.GetAssets())
            {
                _stageIdDataMap.Add(data.Id, data);
            }

            base.InitialiseConfig(config);
        }

        public StageData GetStageData(StageId id)
        {
            return _stageIdDataMap[id];
        }
    }
}