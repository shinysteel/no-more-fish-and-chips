using PrimeTween;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class FadeUIAnimation : SequenceUIAnimation
    {
        [SerializeField] private FadeSettings _settings;

        public override Sequence CreateShowSequence(UIAnimationParams parameters)
        {
            return Sequence.Create(Tween.Alpha(parameters.CanvasGroup, startValue: 0f, endValue: 1f, _settings.Duration, _settings.Ease));
        }

        public override Sequence CreateHideSequence(UIAnimationParams parameters)
        {
            return Sequence.Create(Tween.Alpha(parameters.CanvasGroup, startValue: 1f, endValue: 0f, _settings.Duration, _settings.Ease));
        }
    }
}