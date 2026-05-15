using FishFlingers.Entities;
using FishFlingers.Inventories;
using FishFlingers.Items;
using FishFlingers.States;
using PrimeTween;
using ShinyOwl.Common.Utils;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class ItemActionsView : MonoBehaviour
    {
        // Visualises an action that can be executed
        [Serializable]
        private class ActionView
        {
            [SerializeField] private Button _button;
            [SerializeField] private RectTransform _rectTransform;
            [SerializeField] private GameObject _container;
            [SerializeField] private Image _iconImage;

            public Button Button => _button;

            private Tween _showTween;

            private const float ShowDuration = 0.1f;

            public void Setup(ItemActionData data)
            {
                _iconImage.sprite = data?.HotkeySprite;
            }

            public void Show(bool show)
            {
                _container.gameObject.SetActive(show);

                if (show)
                {
                    _showTween = Tween.UIAnchoredPosition(_rectTransform, endValue: Vector2.zero, duration: ShowDuration, ease: Ease.OutBack);
                }
                else
                {
                    _showTween.Stop();
                    _rectTransform.anchoredPosition = Vector2.down * _rectTransform.rect.size.y * 2f;
                }
            }
        }

        // If there are more actions in the future we should consider a list, but for now this is reasonable
        [SerializeField] private ActionView _leftClickActionView;
        [SerializeField] private ActionView _rightClickActionView;

        private GameplayContext _context;

        public void Setup(GameplayContext context)
        {
            _context = context;

            HandleHotbarSelectedChanged(_context.LocalPlayer.Hotbar.SelectedSlot);
            _context.LocalPlayer.Hotbar.OnSelectedChanged += HandleHotbarSelectedChanged;
        }

        private void OnDestroy()
        {
            if (_context.LocalPlayer?.Hotbar != null)
            {
                _context.LocalPlayer.Hotbar.OnSelectedChanged -= HandleHotbarSelectedChanged;
            }
        }

        private void HandleHotbarSelectedChanged(HotbarSlot slot)
        {
            _leftClickActionView.Setup(slot.InventoryItem?.ItemInstance.Data.LeftClickAction);
            _rightClickActionView.Setup(slot.InventoryItem?.ItemInstance.Data.RightClickAction);

            _leftClickActionView.Show(slot.InventoryItem?.ItemInstance.Data.LeftClickAction != null);
            _rightClickActionView.Show(slot.InventoryItem?.ItemInstance.Data.RightClickAction != null);
        }
    }
}