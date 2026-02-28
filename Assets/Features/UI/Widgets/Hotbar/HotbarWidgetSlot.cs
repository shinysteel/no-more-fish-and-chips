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
        [SerializeField] private Button _button;

        private PoolManager _poolManager;

        private GameplayContext _context;

        private int _index = -1;
        public int Index => _index;

        private UnitItemView _unitItemView;

        public InventoryItem InventoryItem => _view.InventoryItem;
        public CellOutline CellOutline => _view.CellOutline;

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _button.onClick.AddListener(Pressed);
        }

        public void Setup(GameplayContext context, int index)
        {
            _context = context;
            
            _view.Setup(context);

            _index = index;

            _view.CellOutline.SetColor(CellOutline.EColor.Default);
            _view.CellOutline.SetEnabled(true, true, true, true);
        }

        private void Pressed()
        {
            if (_index < 0)
            {
                return;
            }

            _context.LocalPlayer.Hotbar.SetSlot(_index, null);
        }

        public void SetTransform(Vector2 position, Vector2 size)
        {
            _view.SetTransform(position, size);
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
                _unitItemView = _poolManager.Get<UnitItemView>(new SpawnParams() { Parent = transform });
                _unitItemView.SetSlotSize(_view.RectTransform.sizeDelta);
            }

            _unitItemView.Setup(item);
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
            _view.OnDestroy();

            ReturnUnitItemView();
        }

        public void OnTakenFromPool()
        { }
    }
}