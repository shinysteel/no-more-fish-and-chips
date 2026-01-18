using ShinyOwl.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishFlingers.UI
{
    public abstract class UIElement : MonoBehaviour
    {
        [SerializeField] private UIAnimation _uiAnimation;
        [SerializeField] protected RectTransform _rectTransform;
        [SerializeField] protected CanvasGroup _canvasGroup;

        protected Canvas _canvas;
        protected bool _isShowing;

        protected bool _isTransitioning;

        public RectTransform RectTransform => _rectTransform;
        public Canvas Canvas => _canvas;

        public bool IsShowing => _isShowing;
        public bool IsVisible => gameObject.activeSelf;

        public virtual void Load(Canvas canvas)
        {
            _canvas = canvas;
        }

        /// <summary>
        /// Starts showing the element. If the element has an ongoing Show request, it will ignore further requests until done
        /// </summary>
        public virtual void Show(Action onComplete)
        {
            _isShowing = true;
            _uiAnimation.Show(new UIAnimationParams(onComplete, gameObject, _canvasGroup));
        }

        /// <summary>
        /// Starts hiding the element. If the element has an ongoing Hide request, it will ignore further requests until done
        /// </summary>
        public virtual void Hide(Action onComplete)
        {
            _isShowing = false;
            _uiAnimation.Hide(new UIAnimationParams(onComplete, gameObject, _canvasGroup));
        }

        public virtual void Unload()
        {
            _canvas = null;
        }
    }
}