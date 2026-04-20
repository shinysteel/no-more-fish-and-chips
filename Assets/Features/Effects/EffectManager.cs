using UnityEngine;

namespace FishFlingers.Effects
{
    public interface IEffectManagerListener
    { }

    public class EffectManager : GameSystem<IEffectManagerListener>
    {
        private EffectManagerConfig _config;

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.EffectManagerConfig;

            base.Initialise(config);
        }
    }
}