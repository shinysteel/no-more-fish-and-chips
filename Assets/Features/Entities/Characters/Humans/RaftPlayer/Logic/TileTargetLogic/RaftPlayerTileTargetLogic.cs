using FishFlingers.Cameras;
using FishFlingers.Instantiating;
using FishFlingers.Inventories;
using FishFlingers.States;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

namespace FishFlingers.Entities
{
    public class RaftPlayerTileTarget
    {
        private GameplayContext _context;

        private Vector2Int _cell;
        private Tile _tile;

        public Vector2Int Cell => _cell;
        public Tile Tile => _tile;

        public event Action OnChanged;

        public RaftPlayerTileTarget(GameplayContext context)
        {
            _context = context;

            _cell = Vector2Int.one * int.MinValue;

            _context.Raft.OnTileChanged += HandleTileChanged;
        }

        ~RaftPlayerTileTarget()
        {
            if (_context.Raft != null)
            {
                _context.Raft.OnTileChanged -= HandleTileChanged;
            }
        }

        public void SetCell(Vector2Int cell)
        {
            if (_cell == cell)
            {
                return;
            }

            _cell = cell;

            // Refresh _tile whenever _cell changes
            _context.Raft.Tiles.TryGetValue(_cell, out Tile tile);
            HandleTileChanged(_cell, tile);
        }

        private void HandleTileChanged(Vector2Int cell, Tile tile)
        {
            if (_cell != cell)
            {
                return;
            }

            _tile = tile;

            OnChanged?.Invoke();
        }

        public bool CanBuild()
        {
            return CanBuildTile() || CanBuildStructure();
        }

        public bool CanBuildTile()
        {
            return _tile == null;
        }

        public bool CanBuildStructure()
        {
            return _tile != null && _tile.Structure == null;
        }
    }

    public class RaftPlayerTileTargetLogic
    {
        private CameraManager _cameraManager;

        private GameplayContext _context;

        private RaftPlayerTileTargetLogicSettings _settings;

        private RaftPlayerTileTargetVisual _targetVisual;

        private RaftPlayerTileTarget _target;
        public RaftPlayerTileTarget Target => _target;

        private bool _showingTarget;

        private const float MaxRange = 1f;

        public event Action<RaftPlayerTileTarget> OnTargetChanged;

        // Scales for the target depending on context
        private static readonly Vector3 StructureVisualScale = new Vector3(0.75f, 0.25f, 0.75f);
        private static readonly Vector3 TileVisualScale = new Vector3(1f, 0.25f, 1f);

        private Tween _fadeTween;
        private const float FadeDuration = 0.1f;

        private const float VisualMaxAlpha = 0.4f; // Equivalent to ~102 in color32

        public RaftPlayerTileTargetLogic(GameplayContext context, RaftPlayerTileTargetLogicSettings settings)
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();

            _context = context;

            _settings = settings;

            _targetVisual = Object.Instantiate(_settings.TargetVisualPrefab);

            _target = new RaftPlayerTileTarget(context);
            _target.OnChanged += HandleTargetChanged;

            HandleHotbarSelectedSlotChanged(_context.LocalPlayer.Hotbar.SelectedSlot);
            _context.LocalPlayer.Hotbar.OnSelectedChanged += HandleHotbarSelectedSlotChanged;
        }

        ~RaftPlayerTileTargetLogic()
        { 
            if (_context.LocalPlayer != null)
            {
                _context.LocalPlayer.Hotbar.OnSelectedChanged -= HandleHotbarSelectedSlotChanged;
            }
        }

        private void HandleTargetChanged()
        {
            if (_showingTarget)
            {
                RefreshVisualColor();
            }

            // Passes along the event from Target -> Logic -> Listener
            OnTargetChanged?.Invoke(_target);
        }

        private void HandleHotbarSelectedSlotChanged(HotbarSlot slot)
        {
            _showingTarget = slot.InventoryItem?.ItemInstance.Data.ShowsTileTarget ?? false;

            _fadeTween.Stop();

            // Fade in or out the target based on if we are using it or not
            float startValue = _targetVisual.Material.color.a;
            float endValue = _showingTarget ? VisualMaxAlpha : 0f;
            Action<float> onValueChange = (float alpha) => _targetVisual.SetAlpha(alpha);

            _fadeTween = Tween.Custom(startValue: startValue, endValue: endValue, duration: FadeDuration, onValueChange: onValueChange);

            if (_showingTarget)
            {
                RefreshVisualColor();
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
            Ray ray = _cameraManager.MainCamera.ScreenPointToRay(_context.LocalPlayer.InputLogic.GameplayMouse);

            // Have the plane sit at the player's origin so that y does not influence the target
            Plane plane = new Plane(Vector3.up, _context.LocalPlayer.transform.position);

            // Face the cursor
            if (!plane.Raycast(ray, out float distance))
            {
                return;
            }

            Vector3 toPoint = (ray.GetPoint(distance) - _context.LocalPlayer.transform.position);

            // Target the cell x units away from us in the direction we are facing
            Vector3 position = _context.LocalPlayer.transform.position + toPoint.normalized * Mathf.Min(toPoint.magnitude, MaxRange);

            Vector2Int cell = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
            
            // We only care if the cell has changed
            if (_target.Cell == cell)
            {
                return;
            }

            _target.SetCell(cell);
        }

        /// <summary>
        /// Transforms the visual based on whether we are targeting a tile or not
        /// </summary>
        private void TransformVisualTick()
        {
            Vector3 scale;
            Vector3 position = _context.Raft.Queries.CellToWorldPosition(_target.Cell);

            if (_target.Tile != null)
            {
                scale = StructureVisualScale;
                position.y = _target.Tile.GetSurfaceY() + scale.y * 0.5f;
            }
            else
            {
                scale = TileVisualScale;
                position.y = 0f;
            }

            _targetVisual.SetVisualScale(scale);
            _targetVisual.transform.position = position;
        }

        private void RefreshVisualColor()
        {
            _targetVisual.SetColor(_target.CanBuild() ? _settings.ValidColor : _settings.InvalidColor);
        }
    }
}