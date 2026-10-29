using NoMoreFishAndChips.Pools;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class ChatMessageView : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private CanvasGroup _canvasGroup;

        private Chat _chat;

        private float _timer;
        private float _alpha;

        private const float Duration = 5f;

        public void Setup(Chat chat, string text)
        {
            _chat = chat;

            _text.text = text;

            _timer = 0f;
            _alpha = 1f;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            
            if (_timer - Time.deltaTime < Duration && _timer >= Duration)
            {
                Tween.Custom(startValue: 1f, endValue: 0f, duration: 0.25f, onValueChange: (float value) => _alpha = value);
            }

            if (_chat.InUse)
            {
                _canvasGroup.alpha = 1f;
            }
            else
            {
                _canvasGroup.alpha = _alpha;
            }
        }

        public void OnReturnedToPool()
        { }

        public void OnTakenFromPool()
        { }
    }
}