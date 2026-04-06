using FishFlingers.Inventories;
using FishFlingers.Pools;
using FishFlingers.States;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class HotbarWidgetSlot : MonoBehaviour, ISlotView, IPoolable
    {
        [SerializeField] private SlotView _view;

        private PoolManager _poolManager;

        private int _index = -1;
        public int Index => _index;

        private UnitItemView _unitItemView;

        public InventoryItem InventoryItem => _view.InventoryItem;
        public CellOutline CellOutline => _view.CellOutline;

        private const float SlotSizeScalar = 0.9f;

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public void Setup(GameplayContext context, int index)
        {
            _view.Setup(context);

            _index = index;

            _view.CellOutline.SetColor(CellOutline.EColor.Default);
            _view.CellOutline.SetEnabled(true, true, true, true);
        }

        public void SetTransform(Vector2 position, Vector2 size)
        {
            _view.SetTransform(position, size);

            if (_unitItemView != null)
            {
                RefreshItemViewSize();
            }
        }

        public void SetInventoryItem(InventoryItem item)
        {
            _view.SetInventoryItem(item);

            if (item == null)
            {
                ReturnUnitItemView();
                return;
            }

            if (_unitItemView == null)
            {
                _unitItemView = _poolManager.GetPoolable<UnitItemView>(new SpawnParams() { Parent = transform });
                RefreshItemViewSize();
            }

            _unitItemView.Setup(item);
        }

        private void RefreshItemViewSize()
        {
            _unitItemView.SetSlotSize(_view.RectTransform.rect.size * SlotSizeScalar);
            _unitItemView.RefreshRect();
        }
        
        private void ReturnUnitItemView()
        {
            if (_unitItemView != null)
            {
                _poolManager.ReturnPoolable(_unitItemView);
            }

            _unitItemView = null;
        }

        public void OnReturnedToPool()
        {
            _view.OnDestroy();

            ReturnUnitItemView();
        }

        public void OnTakenFromPool()
        { }
    }
}