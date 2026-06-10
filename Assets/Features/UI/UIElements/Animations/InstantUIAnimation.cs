using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class InstantUIAnimation : UIAnimation
    {
        public override void Show(UIAnimationParams parameters)
        {
            parameters.GameObject.SetActive(true);
            parameters.OnComplete?.Invoke();
        }

        public override void Hide(UIAnimationParams parameters)
        {
            parameters.GameObject.SetActive(false);
            parameters.OnComplete?.Invoke();
        }
    }
}