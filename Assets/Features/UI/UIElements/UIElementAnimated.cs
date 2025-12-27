using System;
using UnityEngine;
using PrimeTween;
using System.Collections.Generic;

namespace FishFlingers.UI
{
    public abstract class UIElementAnimated : UIElement
    {
        private Sequence _showSequence;
        private Sequence _hideSequence;

        // Generally will only have one callback that is invoked once Show or Hide ends. Is cumulative
        // to allow repeated calls without losing the previous callbacks
        private List<Action> _pending = new();

        public override void Show(Action onComplete)
        {
            if (_showSequence.isAlive)
            {
                _pending.Add(onComplete);
                return;
            }

            if (_hideSequence.isAlive)
            {
                ExecutePending();
                _hideSequence.Stop();
            }

            _isVisible = true;
            gameObject.SetActive(true);
            _pending.Add(onComplete);

            _showSequence = CreateShowSequence();
            _showSequence.OnComplete(ExecutePending);
        }

        public override void Hide(Action onComplete)
        {
            if (_hideSequence.isAlive)
            {
                _pending.Add(onComplete);
                return;
            }

            if (_showSequence.isAlive)
            {
                ExecutePending();
                _showSequence.Stop();
            }

            _pending.Add(onComplete);

            _hideSequence = CreateHideSequence();
            _hideSequence.OnComplete(() =>
            {
                _isVisible = false;
                gameObject.SetActive(false);
                ExecutePending();
            });
        }

        public override void Unload()
        {
            _showSequence.Stop();
            _hideSequence.Stop();
        }

        public abstract Sequence CreateShowSequence();
        public abstract Sequence CreateHideSequence();

        private void ExecutePending()
        {
            foreach (Action pending in _pending)
            {
                pending?.Invoke();
            }

            _pending.Clear();
        }
    }
}