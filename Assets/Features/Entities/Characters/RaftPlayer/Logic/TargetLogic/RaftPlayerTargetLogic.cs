using FishFlingers.Inventories;
using FishFlingers.States;
using PrimeTween;
using ShinyOwl.Common;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishFlingers.Entities
{
    public class RaftPlayerTarget
    {
        private Vector2Int _cell;
        private RaftTile _tile;

        public Vector2Int Cell => _cell;
        public RaftTile Tile => _tile;

        public RaftPlayerTarget(Vector2Int cell, RaftTile tile)
        {
            _cell = cell;
            _tile = tile;
        }
    }

    public class RaftPlayerTargetLogic
    {
        private GameplayContext _context;

        private RaftPlayerTargetLogicSettings _settings;

        private RaftPlayerTargetVisual _targetVisual;

        private RaftPlayerTarget _target;
        public RaftPlayerTarget Target => _target;

        private bool _showingTarget;

        private const float Range = 1f;

        public event Action<RaftPlayerTarget> OnTargetChanged;

        // Scales for the target depending on context
        private static readonly Vector3 StructureVisualScale = new Vector3(0.75f, 0.25f, 0.75f);
        private static readonly Vector3 TileVisualScale = new Vector3(1f, 0.25f, 1f);

        private Tween _fadeTween;
        private const float FadeDuration = 0.1f;

        private const float VisualMaxAlpha = 0.4f; // Equivalent to ~102 in color32

        public RaftPlayerTargetLogic(GameplayContext context, RaftPlayerTargetLogicSettings settings)
        {
            _context = context;

            _settings = settings;

            _targetVisual = Object.Instantiate(_settings.TargetVisualPrefab);

            _target = new RaftPlayerTarget(Vector2Int.one * int.MinValue, null);

            HandleTileChanged(_target.Cell, _target.Tile);
            _context.Raft.OnTileChanged += HandleTileChanged;

            HandleHotbarSelectedItemChanged(_context.LocalPlayer.Hotbar.SelectedIndex, _context.LocalPlayer.Hotbar.SelectedItem);
            _context.LocalPlayer.Hotbar.OnSelectedChanged += HandleHotbarSelectedItemChanged;
        }

        ~RaftPlayerTargetLogic()
        { 
            if (_context.Raft != null)
            {
                _context.Raft.OnTileChanged -= HandleTileChanged;
            }

            if (_context.LocalPlayer != null)
            {
                _context.LocalPlayer.Hotbar.OnSelectedChanged -= HandleHotbarSelectedItemChanged;
            }
        }

        private void SetTarget(RaftPlayerTarget target)
        {
            _target = target;
            OnTargetChanged?.Invoke(target);

            if (!_showingTarget)
            {
                return;
            }

            bool valid = _target.Tile == null || _target.Tile.Structure == null;
            _targetVisual.SetColor(valid ? _settings.ValidColor : _settings.InvalidColor);
        }

        private void HandleTileChanged(Vector2Int cell, RaftTile tile)
        {
            if (cell != _target.Cell)
            {
                return;
            }

            SetTarget(new RaftPlayerTarget(cell, tile));
        }

        private void HandleHotbarSelectedItemChanged(int index, InventoryItem item)
        {
            _showingTarget = item?.ItemInstance.Data.ShowsTarget ?? false;

            _fadeTween.Stop();

            // Fade in or out the target based on if we are using it or not
            float startValue = _targetVisual.Material.color.a;
            float endValue = _showingTarget ? VisualMaxAlpha : 0f;
            Action<float> onValueChange = (float alpha) => _targetVisual.SetAlpha(alpha);

            _fadeTween = Tween.Custom(startValue: startValue, endValue: endValue, duration: FadeDuration, onValueChange: onValueChange);

            if (_showingTarget)
            {
                _targetVisual.gameObject.SetActive(true);    
            }
            else
            {
                _fadeTween.OnComplete(() => _targetVisual.gameObject.SetActive(false));
            }
        }

        public void Tick()
        {
            // Targets become locked when you can't act
            if (_context.LocalPlayer.CanAct)
            {
                DetermineTargetTick();
            }

            // _isTargeting represents if the selected item displays a target
            if (_showingTarget)
            {
                TransformVisualTick();
            }
        }

        private void DetermineTargetTick()
        {
            Vector3 forward = _context.LocalPlayer.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            // Target the cell x units away from us in the direction we are facing
            Vector3 position = _context.LocalPlayer.transform.position + forward * Range;

            Vector2Int cell = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));

            if (_target.Cell == cell)
            {
                return;
            }

            _context.Raft.Tiles.TryGetValue(cell, out RaftTile tile);

            SetTarget(new RaftPlayerTarget(cell, tile));
        }

        /// <summary>
        /// Transforms the visual based on whether we are targeting a tile or not
        /// </summary>
        private void TransformVisualTick()
        {
            Vector3 scale;
            Vector3 position = _context.Raft.CellToWorldPosition(_target.Cell);

            if (_context.Raft.Tiles.TryGetValue(_target.Cell, out RaftTile tile))
            {
                scale = StructureVisualScale;
                position.y = tile.GetSurfaceY() + scale.y * 0.5f;
            }
            else
            {
                scale = TileVisualScale;
                position.y = 0f;
            }

            _targetVisual.SetVisualScale(scale);
            _targetVisual.transform.position = position;
        }
    }
}