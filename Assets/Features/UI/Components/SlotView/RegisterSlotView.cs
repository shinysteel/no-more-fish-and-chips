using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Pools;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public abstract class RegisterSlotView : SlotView
    {
        private PoolManager _poolManager;

        private UnitItemView _unitItemView;

        private const float SlotSizeScalar = 0.9f;

        protected virtual void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public override void SetInventoryItem(InventoryItem item)
        {
            base.SetInventoryItem(item);

            if (item == null)
            {
                ReturnUnitItemView();
                return;
            }

            if (_unitItemView == null)
            {
                _unitItemView = _poolManager.GetTypedPoolable<UnitItemView>(new SpawnParams() { Parent = transform });
                _unitItemView.SetSlotSize(_rectTransform.rect.size * SlotSizeScalar);
            }

            _unitItemView.Setup(_context, item);
        }

        public override void SetTransform(Vector2 position, Vector2 size)
        {
            base.SetTransform(position, size);

            if (_unitItemView != null)
            {
                _unitItemView.SetSlotSize(_rectTransform.rect.size * SlotSizeScalar);
                _unitItemView.Refresh();
            }
        }

        private void ReturnUnitItemView()
        {
            if (_unitItemView != null)
            {
                _poolManager.ReturnTypedPoolable(_unitItemView);
            }

            _unitItemView = null;
        }

        public void OnReturnedToPool()
        {
            OnDestroy();

            ReturnUnitItemView();
        }

        public void OnTakenFromPool()
        { }
    }
}