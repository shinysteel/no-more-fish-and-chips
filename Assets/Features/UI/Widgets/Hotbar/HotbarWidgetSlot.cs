using FishFlingers.Inventories;
using FishFlingers.Pools;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class HotbarWidgetSlot : MonoBehaviour, IPoolable
    {
        [SerializeField] private SlotView _view;

        public SlotView View => _view;

        private PoolManager _poolManager;

        private int _index = -1;
        public int Index => _index;

        private InventoryItem _inventoryItem;
        private UnitItemView _unitItemView;

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public void Setup(int index)
        {
            _index = index;
            _view.CellOutline.SetColor(CellOutline.EColor.Default);
            _view.CellOutline.SetEnabled(true, true, true, true);
        }

        public void SetInventoryItem(InventoryItem item)
        {
            _inventoryItem = item;

            if (_inventoryItem == null)
            {
                ReturnUnitItemView();
                return;
            }

            if (_unitItemView == null)
            {
                _unitItemView = _poolManager.Get<UnitItemView>(new SpawnParams() { Parent = transform });
                _unitItemView.SetSlotSize(_view.RectTransform.sizeDelta);
            }

            _unitItemView.Setup(_inventoryItem);
        }

        private void ReturnUnitItemView()
        {
            if (_unitItemView != null)
            {
                _poolManager.Return(_unitItemView);
            }

            _unitItemView = null;
        }

        public void OnReturnedToPool()
        {
            ReturnUnitItemView();
        }

        public void OnTakenFromPool()
        { }
    }
}