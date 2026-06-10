using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using System.Threading.Tasks;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public abstract class RegisterWidget<T> : MonoBehaviour where T : RegisterSlotView, ITypedPoolable
    {
        [SerializeField] protected RectTransform _slotsRectTransform;

        protected PoolManager _poolManager;

        protected GameplayContext _context;

        protected T[] _slots;
        protected SlotViewOutliner<T> _outliner;

        public T[] Slots => _slots;

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        protected abstract T[] CreateSlots();

        public virtual async Task SetupAsync(GameplayContext context)
        {
            _context = context;

            _slots = CreateSlots();

            _outliner = new RegisterOutliner<T>(_context, this);

            // It takes one frame for pooled objects to enable when retrieved. Without this delay, slots will have invalid sizeDelta during OnRectTransformDimensionsChange
            while (_slotsRectTransform.rect.size == Vector2.zero)
            {
                await Task.Yield();
            }
        }

        protected virtual void OnDestroy()
        {
            if (_poolManager != null)
            {
                foreach (T slot in _slots)
                {
                    _poolManager.ReturnTypedPoolable(slot);
                }
            }
        }

        private void Update()
        {
            _outliner?.Tick();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_slots == null)
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