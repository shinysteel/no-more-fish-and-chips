using UnityEngine;

namespace FishFlingers.Debugging
{
    public interface IDebugManagerListener
    { }

    public class DebugManager : GameSystem<IDebugManagerListener>
    {
        private DebugManagerConfig _config;

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.DebugManagerConfig;
        }

        public override void Tick()
        {
            FastForwardTick();
        }

        private void FastForwardTick()
        {
            Time.timeScale = Input.GetKey(KeyCode.Tab) ? _config.FastForwardTimeScale : 1f;
        }
    }
}