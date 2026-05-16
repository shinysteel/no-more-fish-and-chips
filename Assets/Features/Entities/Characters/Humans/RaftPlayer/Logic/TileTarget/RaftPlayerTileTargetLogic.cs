using FishFlingers.Cameras;
using FishFlingers.Instantiating;
using FishFlingers.Inventories;
using FishFlingers.States;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
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

        public bool CanRepair()
        {
            return _tile?.EntityHealthModule.Current < _tile?.EntityHealthModule.Max;
        }
    }

    public class RaftPlayerTileTargetLogic
    {
        private CameraManager _cameraManager;

        private RaftPlayer _player;
        private GameplayContext _context;

        private RaftPlayerTileTargetSettings _settings;

        private RaftPlayerTileTargetVisual _targetVisual;

        private RaftPlayerTileTarget _target;
        public RaftPlayerTileTarget Target => _target;

        public event Action<RaftPlayerTileTarget> OnTargetChanged;

        private bool _isBuilding;
        public bool IsBuilding => _isBuilding;

        private const float RepairRange = 1f;

        public RaftPlayerTileTargetLogic(RaftPlayer player, GameplayContext context)
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();

            _player = player;
            _context = context;

            _settings = _player.DefinitionData.TileTargetSettings;

            _targetVisual = Object.Instantiate(_settings.TargetVisualPrefab);

            _target = new RaftPlayerTileTarget(context);
            _target.OnChanged += HandleTargetChanged;

            HandleHotbarSelectedSlotChanged(_player.Hotbar.SelectedSlot);
            _player.Hotbar.OnSelectedChanged += HandleHotbarSelectedSlotChanged;
        }

        ~RaftPlayerTileTargetLogic()
        {
            _target.OnChanged -= HandleTargetChanged;

            if (_player != null)
            {
                _player.Hotbar.OnSelectedChanged -= HandleHotbarSelectedSlotChanged;
            }
        }

        private void HandleTargetChanged()
        {
            RefreshVisual();

            // Passes along the event from Target -> Logic -> Listener
            OnTargetChanged?.Invoke(_target);
        }

        private void HandleHotbarSelectedSlotChanged(HotbarSlot slot)
        {
            RefreshVisual();
        }

        public void Tick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            DetermineTargetTick();
            TransformVisualTick();
        }

        private void DetermineTargetTick()
        {
            Vector2Int newTargetCell;

            if (!_isBuilding)
            {
                Vector2Int playerCell = _context.Raft.Queries.WorldPositionToCell(_player.transform.position);

                List<Tile> tiles = ListPool<Tile>.Get();

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (_context.Raft.Tiles.TryGetValue(playerCell + new Vector2Int(i, j), out Tile tile))
                        {
                            tiles.Add(tile);
                        }
                    }
                }

                // Find the closest tile that can be repaired
                Tile closestTile = tiles
                    .Where(tile => tile.EntityHealthModule.Current < tile.EntityHealthModule.Max && Vector3.Distance(tile.transform.position, _player.transform.position) < RepairRange)
                    .OrderBy(tile => Vector3.Distance(tile.transform.position, _player.transform.position))
                    .FirstOrDefault();

                ListPool<Tile>.Release(tiles);

                newTargetCell = closestTile?.Cell ?? playerCell;                
            }
            else
            {
                newTargetCell = _context.Raft.Queries.WorldPositionToCell(_player.transform.position + _player.transform.forward * 1f);
            }
            
            // We only care if the cell has changed
            if (_target.Cell == newTargetCell)
            {
                return;
            }

            // Mark the target as dirty by changing its cell, which will cause RefreshVisual to be invoked
            _target.SetCell(newTargetCell);
        }

        private void TransformVisualTick()
        {
            Vector3 position = _context.Raft.Queries.CellToWorldPosition(_target.Cell);

            if (_target.Tile != null)
            {
                position.y = _target.Tile.GetSurfaceY();
            }

            _targetVisual.transform.position = position;
        }

        private void RefreshVisual()
        {
            if (_isBuilding)
            {
                if (_target.CanBuildTile())
                {
                    _targetVisual.SetVisual(RaftPlayerTileTargetVisual.EVisual.TileScaffold);
                    _targetVisual.SetColor(RaftPlayerTileTargetVisual.EColor.Valid);
                }
                else
                {
                    _targetVisual.SetVisual(RaftPlayerTileTargetVisual.EVisual.StructureScaffold);
                    _targetVisual.SetColor(_target.CanBuildStructure() ? RaftPlayerTileTargetVisual.EColor.Valid : RaftPlayerTileTargetVisual.EColor.Invalid);
                }
            }
            else
            {
                _targetVisual.SetVisual(RaftPlayerTileTargetVisual.EVisual.None);
            }
        }

        public void SetIsBuilding(bool building)
        {
            if (_isBuilding == building)
            {
                return;
            }

            _isBuilding = building;

            RefreshVisual();
        }
    }
}