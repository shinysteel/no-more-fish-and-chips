using NoMoreFishAndChips.UI;
using ShinyOwl.Common;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.UI.Transitions
{
    public interface ITransitionManagerListener
    { }

    public class TransitionManager : GameSystem<ITransitionManagerListener>
    {
        private TransitionManagerConfig _config;

        private UIManager _uiManager;

        private FadeOverlay _fadeOverlay;

        public bool IsShowing => _fadeOverlay.IsShowing;

        public override void InitialiseConfig(GameManagerConfig config)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();

            _config = config.TransitionManagerConfig;

            _uiManager.CreateScreenUIAsync(_uiManager.Config.FadeOverlayPrefab, UILayer.Overlays).completed += (FadeOverlay overlay) =>
            {
                _fadeOverlay = overlay;
            };

            base.InitialiseConfig(config);
        }

        public override void Dispose()
        {
            base.Dispose();
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