using FishFlingers.Inventories;
using FishFlingers.Pools;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class HotbarSlot : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;

        private PoolManager _poolManager;

        private UnitItemView _unitItemView;

        public void SetInventoryItem(InventoryItem item)
        {
            // Somehow assigning in Awake is not soon enough, so we need to do it here
            _poolManager ??= GameManager.Instance.Get<PoolManager>();

            if (item != null)
            {
                if (_unitItemView == null)
                {
                    _unitItemView = _poolManager.GetPoolable<UnitItemView>(new SpawnParams() { Parent = transform });
                    _unitItemView.SetSlotSize(_rectTransform.sizeDelta);
                }

                _unitItemView.Setup(item);
            }
            else
            {
                if (_unitItemView != null)
                {
                    _poolManager.ReturnPoolable(_unitItemView);
                    _unitItemView = null;
                }
            }
        }

        public void SetSelected(bool selected)
        {
        }
    }
}