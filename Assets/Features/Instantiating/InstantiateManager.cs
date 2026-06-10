using PurrNet;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Instantiating
{
    public interface IInstantiateManagerListener
    {
        void OnComponentInstantiated(Component component) { }
        void OnComponentDestroyed(Component component) { }
    }

    // Components that want to broadcast their lifecycle can do so through this manager
    public class InstantiateManager : GameSystem<IInstantiateManagerListener>
    {
        private InstantiateManagerConfig _config;

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.InstantiateManagerConfig;

            base.Initialise(config);
        }

        public void RaiseComponentInstantiated(Component component) => NotifyComponentInstantiated(component);
        public void RaiseComponentDestroyed(Component component) => NotifyComponentDestroyed(component);

        private void NotifyComponentInstantiated(Component component) => Listeners.Dispatch(listener => listener.OnComponentInstantiated(component));
        private void NotifyComponentDestroyed(Component component) => Listeners.Dispatch(listener => listener.OnComponentDestroyed(component));
    }
}