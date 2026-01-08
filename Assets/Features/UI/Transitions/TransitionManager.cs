using FishFlingers.UI;
using ShinyOwl.Common;
using System;
using UnityEngine;

namespace FishFlingers.UI.Transitions
{
    public interface ITransitionManagerListener
    { }

    public class TransitionManager : GameSystem<ITransitionManagerListener>
    {
        private TransitionManagerConfig _config;
        public TransitionManagerConfig Config => _config;

        private UIManager _uiManager;

        private FadeOverlay _fadeOverlay;

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.TransitionManagerConfig;

            _uiManager = GameManager.Instance.Get<UIManager>();

            _uiManager.CreateUIElementAsync(_uiManager.Config.FadeOverlay, UILayer.Overlay).completed += (UIElement element) =>
            {
                _fadeOverlay = (FadeOverlay)element;
            };

            base.Initialise(config);
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }

        public void CoverScreen(Action onComplete)
        {
            _fadeOverlay.Show(onComplete);
        }

        public void UncoverScreen(Action onComplete)
        {
            _fadeOverlay.Hide(onComplete);
        }
    }
}