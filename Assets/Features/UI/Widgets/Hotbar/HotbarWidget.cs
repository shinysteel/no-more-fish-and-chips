using FishFlingers.Inventories;
using FishFlingers.Pools;
using FishFlingers.States;
using ShinyOwl.Common;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using FishFlingers.Entities;

namespace FishFlingers.UI
{
    public class HotbarWidget : MonoBehaviour
    {
        [SerializeField] private RectTransform _slotsRectTransform;

        private PoolManager _poolManager;

        private Hotbar _hotbar;

        private HotbarWidgetSlot[] _slots;
        public HotbarWidgetSlot[] Slots => _slots;

        private HotbarOutliner _hotbarOutliner;

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public async Task SetupAsync(GameplayContext context)
        {
            _hotbar = context.LocalPlayer.Hotbar;

            _slots = new HotbarWidgetSlot[_hotbar.Slots.Count];

            foreach (HotbarSlot slot in _hotbar.Slots)
            {
                _slots[slot.Index] = _poolManager.GetTypedPoolable<HotbarWidgetSlot>(new SpawnParams() { Parent = _slotsRectTransform });
                _slots[slot.Index].Setup(context, slot.Index);
            }

            _hotbarOutliner = new HotbarOutliner(context, this);

            // It takes one frame for pooled objects to enable when retrieved. Without this delay, slots will have invalid sizeDelta during OnRectTransformDimensionsChange
            while (_slotsRectTransform.rect.size == Vector2.zero)
            {
                await Task.Yield();
            }
            
            foreach (HotbarSlot slot in _hotbar.Slots)
            {
                HandleSlotChanged(slot);
            }

            _hotbar.OnSlotChanged += HandleSlotChanged;
        }

        private void OnDestroy()
        {
            if (_poolManager != null)
            {
                foreach (HotbarWidgetSlot slot in _slots)
                {
                    _poolManager.ReturnTypedPoolable(slot);
                }
            }
            
            if (_hotbar != null)
            {
                _hotbar.OnSlotChanged -= HandleSlotChanged;
            }
        }

        private void Update()
        {
            _hotbarOutliner.Tick();
        }

        private void HandleSlotChanged(HotbarSlot slot)
        {
            _slots[slot.Index].SetInventoryItem(slot.InventoryItem);

            _hotbarOutliner.Refresh();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_hotbar == null)
            {
                return;
            }

            RefreshSlots();
        }

        private void RefreshSlots()
        {
            Vector2 size = new Vector2(_slotsRectTransform.rect.width / _slots.Length, _slotsRectTransform.rect.height);
            float pivot = (_slots.Length - 1) / 2f;

            for (int i = 0; i < _slots.Length; i++)
            {
                Vector2 position = new Vector2((i - pivot) * size.x, 0f);
                _slots[i].SetTransform(position, size);
            }
        }
    }
}