using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class Layer : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;

        public RectTransform RectTransform => _rectTransform;

        // UI is created async, so it's important to account for them when determining if a layer is 'in use'
        private int _pendingCreateOps;

        public void ChangePendingCreateOps(int delta)
        {
            _pendingCreateOps += delta;
        }

        public bool InUse()
        {
            return _pendingCreateOps > 0 || _rectTransform.childCount > 0;
        }
    }
}