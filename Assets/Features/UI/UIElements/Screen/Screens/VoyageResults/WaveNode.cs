using NoMoreFishAndChips.Pools;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class WaveNode : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private Image _image;

        [SerializeField] private Color _completeColor;
        [SerializeField] private Color _incompleteColor;

        public void Setup(bool complete)
        {
            _image.color = complete ? _completeColor : _incompleteColor;
        }

        public void OnReturnedToPool()
        { }

        public void OnTakenFromPool()
        { }
    }
}