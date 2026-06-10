using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class WaveMeterCell : MonoBehaviour
    {
        [SerializeField] private Image _image;

        [SerializeField] private Sprite _defaultSprite;
        [SerializeField] private Sprite _emptySprite;

        public void Setup(bool value)
        {
            _image.sprite = value ? _defaultSprite : _emptySprite;
        }
    }
}