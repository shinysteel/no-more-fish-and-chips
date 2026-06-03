using FishFlingers.Inventories;
using FishFlingers.Items;
using FishFlingers.Pools;
using FishFlingers.States;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class HotbarWidgetSlot : MonoBehaviour, ISlotView, ITypedPoolable
    {
        [SerializeField] private SlotView _view;
        [SerializeField] private Image _assignmentImage;
        
        private PoolManager _poolManager;
        private ItemManager _itemManager;

        private GameplayContext _context;

        private int _index = -1;
        public int Index => _index;

        private UnitItemView _unitItemView;

        public InventoryItem InventoryItem => _view.InventoryItem;
        public CellOutline CellOutline => _view.CellOutline;

        private const float SlotSizeScalar = 0.9f;

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
            _itemManager = GameManager.Instance.Get<ItemManager>();
        }

        public void Setup(GameplayContext context, int index)
        {
            _view.Setup(context);

            _context = context;
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

            RefreshAssignmentImage();

            if (item == null)
            {
                ReturnUnitItemView();
                return;
            }

            if (_unitItemView == null)
            {
                _unitItemView = _poolManager.GetTypedPoolable<UnitItemView>(new SpawnParams() { Parent = transform });
                RefreshItemViewSize();
            }

            _unitItemView.Setup(_context, item);
        }

        private void RefreshAssignmentImage()
        {
            if (_view.InventoryItem != null && _context.LocalPlayer.Hotbar.IsItemAssigned(_view.InventoryItem, out int index))
            {
                _assignmentImage.sprite = _itemManager.GetAssignmentSprite(index);
                _assignmentImage.enabled = true;
            }
            else
            {
                _assignmentImage.enabled = false;
            }
        }

        // Invoked when first retrieving the view and by the widget when OnRectTransformDimensionsChange is invoked
        private void RefreshItemViewSize()
        {
            _unitItemView.SetSlotSize(_view.RectTransform.rect.size * SlotSizeScalar);
            _unitItemView.RefreshView();
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
            _view.OnDestroy();

            ReturnUnitItemView();
        }

        public void OnTakenFromPool()
        { }
    }
}