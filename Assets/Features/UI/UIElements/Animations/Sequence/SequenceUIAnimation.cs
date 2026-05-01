using PrimeTween;
using ShinyOwl.Common;
using System;
using UnityEngine;

namespace FishFlingers.UI
{
    public abstract class SequenceUIAnimation : UIAnimation
    {
        private Sequence _showSequence;
        private Sequence _hideSequence;

        public abstract Sequence CreateShowSequence(UIAnimationParams parameters);
        public abstract Sequence CreateHideSequence(UIAnimationParams parameters);

        public override void Show(UIAnimationParams parameters)
        {
            if (_showSequence.isAlive)
            {
                Log.Error("Show sequence is already active");
                return;
            }

            _hideSequence.Stop();

            parameters.CanvasGroup.interactable = false;
            parameters.GameObject.SetActive(true);

            _showSequence = CreateShowSequence(parameters);
            _showSequence.OnComplete(() =>
            {
                parameters.CanvasGroup.interactable = true;
                parameters.OnComplete?.Invoke();
            });
        }

        public override void Hide(UIAnimationParams parameters)
        {
            if (_hideSequence.isAlive)
            {
                Log.Error("Hide sequence is already active");
                return;
            }

            _showSequence.Stop();

            parameters.CanvasGroup.interactable = false;

            _hideSequence = CreateHideSequence(parameters);
            _hideSequence.OnComplete(() =>
            {
                parameters.GameObject.SetActive(false);
                parameters.OnComplete?.Invoke();
            });
        }
    }
}