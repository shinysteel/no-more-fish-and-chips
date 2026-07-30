using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.States;
using PrimeTween;
using ShinyOwl.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NoMoreFishAndChips.Voyages;

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

            HandleStageChanged(_context.VoyageRunner.Voyage?.Stage);
            
            _context.VoyageRunner.OnStageChanged += HandleStageChanged;
                
            HandleWaveIndexChanged(_context.VoyageRunner.WaveIndex);

            _context.VoyageRunner.OnWaveIndexChanged += HandleWaveIndexChanged;
        }

        private void OnDestroy()
        {
            if (_context.VoyageRunner != null)
            {
                _context.VoyageRunner.OnStageChanged -= HandleStageChanged;
                _context.VoyageRunner.OnWaveIndexChanged -= HandleWaveIndexChanged;
            }
        }

        private void HandleStageChanged(IStage stage)
        {
            _rectTransform.sizeDelta = new Vector2(stage != null ? BaseWidth + IndexWidth * stage.Data.Waves.Length : 0f, _rectTransform.sizeDelta.y);
        }

        private void HandleWaveIndexChanged(int index)
        {
            if (_context.VoyageRunner.Voyage == null)
            {
                return;
            }

            _fillTween.Stop();

            _fillTween = Tween.UIFillAmount(_fillImage, endValue: (float)index / _context.VoyageRunner.Voyage.Stage.Data.Waves.Length, duration: 0.5f);
        }
    }
}