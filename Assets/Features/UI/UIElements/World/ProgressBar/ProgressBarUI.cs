using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class ProgressBarUI : WorldUI
    {
        [SerializeField] private Image _fillImage;

        public void SetFillAmount(float amount)
        {
            _fillImage.fillAmount = amount;
        }
    }
}
