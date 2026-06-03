using FishFlingers.Instantiating;
using FishFlingers.Inventories;
using FishFlingers.States;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using UnityEngine;
using UnityEngine.UI;
using FishFlingers.Entities;

namespace FishFlingers.UI
{
    public class HotbarView : MonoBehaviour
    {
        [SerializeField] private Transform _backgroundTransform;
        [SerializeField] private Image _backgroundImage;

        [SerializeField] private HotbarSlotView _slotViewPrefab;

        private GameplayContext _context;

        private Material _backgroundMaterial;

        private HotbarSlotView[] _slotViews;

        // Background shader requires an index that doesn't wrap, so we use a local index here and just use the
        // delta when updating the hotbar's selected index
        private int _selectedIndex;
        private float _selectedIndexBlend;

        private const string SlotsCountName = "_SlotsCount";
        private const string HighlightIndexName = "_HighlightIndex";

        private const float ScrollSpeed = 100f;

        public void Setup(GameplayContext context)
        {
            _context = context;

            Hotbar hotbar = _context.LocalPlayer.Hotbar;

            _slotViews = new HotbarSlotView[hotbar.Slots.Count];

            for (int i = 0; i < _slotViews.Length; i++)
            {
                _slotViews[i] = Instantiate(_slotViewPrefab, transform);
                _slotViews[i].Setup(_context);
            }

            _backgroundMaterial = _backgroundImage.material;
            _backgroundMaterial.SetInt(SlotsCountName, hotbar.Slots.Count);
            
            // Since the background is getting inverse masked, it needs to be last
            _backgroundTransform.SetAsLastSibling();

            foreach (HotbarSlot slot in hotbar.Slots)
            {
                HandleSlotChanged(slot);
            }

            hotbar.OnSlotChanged += HandleSlotChanged;

            _selectedIndex = hotbar.SelectedSlot.Index;
            _selectedIndexBlend = _selectedIndex;

            HandleSelectedChanged(hotbar.SelectedSlot);
            hotbar.OnSelectedChanged += HandleSelectedChanged;
        }

        ~HotbarView()
        {
            if (_context.LocalPlayer?.Hotbar != null)
            {
                _context.LocalPlayer.Hotbar.OnSlotChanged -= HandleSlotChanged;
                _context.LocalPlayer.Hotbar.OnSelectedChanged -= HandleSelectedChanged;
            }
        }

        private void HandleSlotChanged(HotbarSlot slot)
        {
            _slotViews[slot.Index].SetInventoryItem(slot.InventoryItem);
        }

        private void Update()
        {
            BackgroundUpdate();
        }

        private void BackgroundUpdate()
        {
            _selectedIndexBlend = Mathf.Lerp(_selectedIndexBlend, _selectedIndex, ScrollSpeed * Time.deltaTime);

            // The background uses a shader to highlight the selected slot
            _backgroundMaterial.SetFloat(HighlightIndexName, _selectedIndexBlend);
        }

        private void HandleSelectedChanged(HotbarSlot slot)
        {
            int delta = slot.Index - Utils.Math.EuclideanModulo(_selectedIndex, _slotViews.Length);
            _selectedIndex += delta;

            for (int i = 0; i < _slotViews.Length; i++)
            {
                _slotViews[i].SetSelected(i == slot.Index);
            }
        }
    }
}