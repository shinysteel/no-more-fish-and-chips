using FishFlingers.Pools;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class CellOutline : MonoBehaviour, IPoolable
    {
        [SerializeField] private Image _topImage;
        [SerializeField] private Image _leftImage;
        [SerializeField] private Image _bottomImage;
        [SerializeField] private Image _rightImage;

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

        public void SetColor(Color color)
        {
            _topImage.color = color;
            _leftImage.color = color;
            _bottomImage.color = color;
            _rightImage.color = color;
        }

        public void OnTakenFromPool()
        { }

        public void OnReturnedToPool()
        { }
    }
}