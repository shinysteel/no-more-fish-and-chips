using UnityEngine;

namespace NoMoreFishAndChips.Voyages
{
    public interface IVoyageManagerListener
    { }

    public class VoyageManager : GameSystem<IVoyageManagerListener>
    {
        private VoyageManagerConfig _config;

        public override void InitialiseConfig(GameManagerConfig config)
        {
            _config = config.VoyageManagerConfig;

            base.InitialiseConfig(config);
        }
    }
}