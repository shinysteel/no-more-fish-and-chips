using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class HealthBarUI : WorldUI
    {
        [SerializeField] private Image _fillImage;

        [SerializeField] private Gradient _fillGradient;

        private const int BaseWidth = 4;
        private const int HealthWidth = 6;

        public void SetMaxHealth(int health)
        {
            Vector2 size = _rectTransform.sizeDelta;
            size.x = BaseWidth + HealthWidth * health;
            _rectTransform.sizeDelta = size;
        }
        
        public void SetFillAmount(float amount)
        {
            _fillImage.fillAmount = amount;

            _fillImage.color = _fillGradient.Evaluate(amount);
        }
    }
}