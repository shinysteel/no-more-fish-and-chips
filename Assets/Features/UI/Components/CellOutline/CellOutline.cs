using NoMoreFishAndChips.Pools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class CellOutline : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _topImage;
        [SerializeField] private Image _leftImage;
        [SerializeField] private Image _bottomImage;
        [SerializeField] private Image _rightImage;

        [SerializeField] private Color _defaultColor = Color.grey;
        [SerializeField] private Color _highlightColor = Color.white;
        [SerializeField] private Color _validColor = Color.green;
        [SerializeField] private Color _invalidColor = Color.red;

        public RectTransform RectTransform => _rectTransform;

        public enum EColor
        {
            Default,
            Highlighted,
            Positive,
            Negative,
        }

        private void Awake()
        {
            SetEnabled(false, false, false, false);
        }

        public void SetEnabled(bool top, bool left, bool bottom, bool right)
        {
            _topImage.enabled = top;
            _leftImage.enabled = left;
            _bottomImage.enabled = bottom;
            _rightImage.enabled = right;
        }

        public void SetColor(EColor colorEnum)
        {
            Color color = colorEnum switch
            {
                EColor.Default => _defaultColor,
                EColor.Highlighted => _highlightColor,
                EColor.Positive => _validColor,
                EColor.Negative => _invalidColor,
                _ => _defaultColor
            };

            _topImage.color = color;
            _leftImage.color = color;
            _bottomImage.color = color;
            _rightImage.color = color;
        }
    }
}