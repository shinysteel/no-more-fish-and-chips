using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishFlingers.UI
{
    public abstract class UIElement : MonoBehaviour
    {
        protected bool _isVisible;

        [SerializeField] protected RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform;

        public virtual void Load()
        { }

        public virtual void Show(Action onComplete)
        {
            if (_isVisible)
            {
                onComplete?.Invoke();
                return;
            }

            _isVisible = true;
            gameObject.SetActive(true);
            onComplete?.Invoke();
        }

        public virtual void Hide(Action onComplete)
        {
            if (!_isVisible)
            {
                onComplete?.Invoke();
                return;
            }

            _isVisible = false;
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }

        public virtual void Unload()
        { }
    }
}