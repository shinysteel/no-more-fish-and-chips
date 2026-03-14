using UnityEngine;

namespace FishFlingers.Saving
{
    public interface ISaveManagerListener
    { }

    public class SaveManager : GameSystem<ISaveManagerListener>
    {
        private SaveManagerConfig _config;

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.SaveManagerConfig;

            base.Initialise(config);
        }
    }
}