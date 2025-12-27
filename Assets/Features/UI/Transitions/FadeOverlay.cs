using PrimeTween;
using ShinyOwl.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI.Transitions
{
    public class FadeOverlay : UIElementAnimated
    {
        [SerializeField] private Image _image;

        private TransitionManager _transitionManager;

        public override void Load()
        {
            _transitionManager = GameManager.Instance.Get<TransitionManager>();
        }

        public override Sequence CreateShowSequence()
        {
            return Sequence.Create(Tween.Alpha(_image, startValue: 0f, endValue: 1f, duration: _transitionManager.Config.Duration, ease: _transitionManager.Config.Ease));
        }

        public override Sequence CreateHideSequence()
        {
            return Sequence.Create(Tween.Alpha(_image, startValue: 1f, endValue: 0f, duration: _transitionManager.Config.Duration, ease: _transitionManager.Config.Ease));
        }
    }
}