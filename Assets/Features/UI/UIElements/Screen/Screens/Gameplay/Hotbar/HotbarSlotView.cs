using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class HotbarSlotView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;

        private PoolManager _poolManager;

        private GameplayContext _context;

        private UnitItemView _unitItemView;

        public void Setup(GameplayContext context)
        {
            _context = context;
        }

        public void SetInventoryItem(InventoryItem item)
        {
            // Somehow assigning in Awake is not soon enough, so we need to do it here
            _poolManager ??= GameManager.Instance.Get<PoolManager>();

            if (item != null)
            {
                if (_unitItemView == null)
                {
                    _unitItemView = _poolManager.GetTypedPoolable<UnitItemView>(new SpawnParams() { Parent = transform });
                    _unitItemView.SetSlotSize(_rectTransform.sizeDelta);
                }

                _unitItemView.Setup(_context, item);
            }
            else
            {
                if (_unitItemView != null)
                {
                    _poolManager.ReturnTypedPoolable(_unitItemView);
                    _unitItemView = null;
                }
            }
        }
    }
}