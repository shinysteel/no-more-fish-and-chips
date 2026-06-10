using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.States;
using PrimeTween;
using ShinyOwl.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class WaveMeter : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _fillImage;

        private WaveSpawner _waveSpawner;

        private Tween _fillTween;

        private const float BaseWidth = 80f;
        private const float IndexWidth = 16f;

        public void Setup(GameplayContext context)
        {
            _waveSpawner = context.WaveSpawner;

            _rectTransform.sizeDelta = new Vector2(BaseWidth + IndexWidth * _waveSpawner.StageData.Waves.Length, _rectTransform.sizeDelta.y);

            HandleWaveIndexChanged(_waveSpawner.WaveIndex);

            _waveSpawner.OnWaveIndexChanged += HandleWaveIndexChanged;
        }

        private void OnDestroy()
        {
            if (_waveSpawner != null)
            {
                _waveSpawner.OnWaveIndexChanged -= HandleWaveIndexChanged;
            }
        }

        private void HandleWaveIndexChanged(int index)
        {
            _fillTween.Stop();

            _fillTween = Tween.UIFillAmount(_fillImage, endValue: (float)index / _waveSpawner.StageData.Waves.Length, duration: 0.5f);
        }
    }
}