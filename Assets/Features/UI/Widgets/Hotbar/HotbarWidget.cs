using FishFlingers.Inventories;
using FishFlingers.Pools;
using FishFlingers.States;
using ShinyOwl.Common;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class HotbarWidget : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;

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

            for (int i = 0; i < _hotbar.Slots.Count; i++)
            {
                _slots[i] = _poolManager.Get<HotbarWidgetSlot>(new SpawnParams() { Parent = transform });
                _slots[i].Setup(context, i);
            }

            _hotbarOutliner = new HotbarOutliner(context, this);

            // It takes one frame for pooled objects to enable when retrieved. Without this delay, slots will have invalid sizeDelta during OnRectTransformDimensionsChange
            while (_rectTransform.rect.size == Vector2.zero)
            {
                await Task.Yield();
            }
            
            for (int i = 0; i < _hotbar.Slots.Count; i++)
            {
                HandleSlotChanged(i, _hotbar.Slots[i]);
            }

            _hotbar.OnSlotChanged += HandleSlotChanged;
        }

        private void OnDestroy()
        {
            if (_poolManager != null)
            {
                foreach (HotbarWidgetSlot slot in _slots)
                {
                    _poolManager.Return(slot);
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

        private void HandleSlotChanged(int index, InventoryItem item)
        {
            _slots[index].SetInventoryItem(item);

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
            Vector2 size = new Vector2(_rectTransform.rect.width / _slots.Length, _rectTransform.rect.height);
            float pivot = (_slots.Length - 1) / 2f;

            for (int i = 0; i < _slots.Length; i++)
            {
                Vector2 position = new Vector2((i - pivot) * size.x, 0f);
                _slots[i].SetTransform(position, size);
            }
        }
    }
}