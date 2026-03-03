using FishFlingers.Inventories;
using FishFlingers.States;
using PrimeTween;
using ShinyOwl.Common;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishFlingers.Entities
{
    public class RaftPlayerTargetLogic
    {
        private GameplayContext _context;

        private RaftPlayerTarget _target;

        private bool _isTargeting;

        private Vector2Int _targetCell;
        public Vector2Int TargetCell => _targetCell;

        private const float Range = 1f;

        // Scales for the target depending on context
        private static readonly Vector3 StructureVisualScale = new Vector3(0.75f, 0.25f, 0.75f);
        private static readonly Vector3 TileVisualScale = new Vector3(1f, 0.25f, 1f);

        private Tween _fadeTween;
        private const float FadeDuration = 0.1f;

        public RaftPlayerTargetLogic(GameplayContext context, RaftPlayerTarget targetPrefab)
        {
            _context = context;

            _target = Object.Instantiate(targetPrefab);

            HandleHotbarSelectedItemChanged(_context.LocalPlayer.Hotbar.SelectedIndex, _context.LocalPlayer.Hotbar.SelectedItem);
            _context.LocalPlayer.Hotbar.OnSelectedChanged += HandleHotbarSelectedItemChanged;
        }

        ~RaftPlayerTargetLogic()
        {
            if (_context.LocalPlayer?.Hotbar != null)
            {
                _context.LocalPlayer.Hotbar.OnSelectedChanged -= HandleHotbarSelectedItemChanged;
            }
        }

        private void HandleHotbarSelectedItemChanged(int index, InventoryItem item)
        {
            _isTargeting = item?.ItemInstance.Data.DisplaysTarget ?? false;

            _fadeTween.Stop();

            Action<float> fade = (float value) => _target.SetAlphaBlend(value);

            // Fade in or out the target based on if we are using it or not
            if (_isTargeting)
            {
                _target.gameObject.SetActive(true);

                _fadeTween = Tween.Custom(startValue: _target.GetAlphaBlend(), endValue: 1f, duration: FadeDuration, onValueChange: fade);
            }
            else
            {
                _fadeTween = Tween.Custom(startValue: _target.GetAlphaBlend(), endValue: 0f, duration: FadeDuration, onValueChange: fade)
                    .OnComplete(() => _target.gameObject.SetActive(false));
            }
        }

        public void Tick()
        {
            // _isTargeting represents if the selected item displays a target
            if (!_isTargeting)
            {
                return;
            }

            TransformVisualTick();

            // Targets become locked when you can't act
            if (!_context.LocalPlayer.CanAct)
            {
                return;
            }

            DetermineTargetTick();
        }

        private void DetermineTargetTick()
        {
            Vector3 forward = _context.LocalPlayer.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 position = _context.LocalPlayer.transform.position + forward * Range;

            // Target the cell x units away from us in the direction we are facing
            _targetCell = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        }

        /// <summary>
        /// Transforms the visual based on whether we are targeting a tile or not
        /// </summary>
        private void TransformVisualTick()
        {
            Vector3 scale;
            Vector3 position = _context.Raft.CellToWorldPosition(_targetCell);

            if (_context.Raft.Tiles.TryGetValue(_targetCell, out RaftTile tile))
            {
                scale = StructureVisualScale;
                position.y = tile.GetSurfaceY() + scale.y * 0.5f;
            }
            else
            {
                scale = TileVisualScale;
                position.y = 0f;
            }

            _target.SetVisualScale(scale);
            _target.transform.position = position;
        }
    }
}