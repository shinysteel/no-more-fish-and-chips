using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class Hotbar : MonoBehaviour
    {
        [SerializeField] private Transform _backgroundTransform;
        [SerializeField] private Image _backgroundImage;

        [SerializeField] private HotbarSlot _slotPrefab;

        private Material _backgroundMaterial;

        private HotbarSlot[] _slots;

        private int _selectedIndex;
        private float _selectedIndexBlend;

        private const int SlotsCount = 3;

        private const string SlotsCountName = "_SlotsCount";
        private const string HighlightIndexName = "_HighlightIndex";

        private void Start()
        {
            _backgroundMaterial = _backgroundImage.material;
            _backgroundMaterial.SetInt(SlotsCountName, SlotsCount);

            _slots = new HotbarSlot[SlotsCount];

            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = Instantiate(_slotPrefab, transform);
            }

            // Since the background is getting inverse masked, it needs to be last
            _backgroundTransform.SetAsLastSibling();

            RefreshSlots();
        }

        private void Update()
        {
            ScrollUpdate();
            BackgroundUpdate();
        }

        private void ScrollUpdate()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll > 0f)
            {
                ChangeSelectedSlot(-1);
            }
            else if (scroll < 0f)
            {
                ChangeSelectedSlot(1);
            }
        }

        private void BackgroundUpdate()
        {
            float speed = 50f;
            _selectedIndexBlend = Mathf.Lerp(_selectedIndexBlend, _selectedIndex, speed * Time.deltaTime);

            // The background uses a shader to highlight the selected slot
            _backgroundMaterial.SetFloat(HighlightIndexName, _selectedIndexBlend);
        }

        private void ChangeSelectedSlot(int delta)
        {
            if (delta == 0)
            {
                return;
            }

            _selectedIndex += delta;

            RefreshSlots();
        }

        private void RefreshSlots()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i].SetSelected(i == _selectedIndex);
            }
        }
    }
}