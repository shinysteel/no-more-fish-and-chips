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

        private GameplayContext _context;

        private Tween _fillTween;

        private const float BaseWidth = 80f;
        private const float IndexWidth = 16f;

        public void Setup(GameplayContext context)
        {
            _context = context;

            // _rectTransform.sizeDelta = new Vector2(BaseWidth + IndexWidth * _waveRunner.StageData.Waves.Length, _rectTransform.sizeDelta.y);

            //HandleWaveIndexChanged(_waveRunner.WaveIndex);

            //_waveRunner.OnWaveIndexChanged += HandleWaveIndexChanged;
        }

        private void OnDestroy()
        {
            //if (_waveRunner != null)
            //{
            //    _waveRunner.OnWaveIndexChanged -= HandleWaveIndexChanged;
            //}
        }

        private void HandleWaveIndexChanged(int index)
        {
            //_fillTween.Stop();

            //_fillTween = Tween.UIFillAmount(_fillImage, endValue: (float)index / _waveRunner.StageData.Waves.Length, duration: 0.5f);
        }
    }
}