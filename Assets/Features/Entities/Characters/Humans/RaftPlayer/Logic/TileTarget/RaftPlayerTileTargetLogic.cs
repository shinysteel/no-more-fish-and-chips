using NoMoreFishAndChips.Cameras;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Instantiating;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.States;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerTileTargetLogic : RaftPlayerLogic
    {
        private CameraManager _cameraManager;
        private EnvironmentManager _environmentManager;

        private RaftPlayerTileTargetSettings _settings;

        private Prop _targetProp;

        private RaftTileTarget _target;
        public RaftTileTarget Target => _target;

        public event Action<RaftTileTarget> OnTargetChanged;

        private bool _isBuilding;
        public bool IsBuilding => _isBuilding;

        private const float RepairRange = 1f;

        public RaftPlayerTileTargetLogic(RaftPlayer player) : base(player)
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _environmentManager = GameManager.Instance.Get<EnvironmentManager>();

            _settings = _player.DefinitionData.TileTargetSettings;
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            _target = new RaftTileTarget(_context);
            _target.OnChanged += HandleTargetChanged;

            HandleHotbarSelectedSlotChanged(_player.Hotbar.SelectedSlot);
            _player.Hotbar.OnSelectedChanged += HandleHotbarSelectedSlotChanged;
        }

        public override void OnDespawned()
        {
            _target.OnChanged -= HandleTargetChanged;

            if (_player != null)
            {
                _player.Hotbar.OnSelectedChanged -= HandleHotbarSelectedSlotChanged;
            }
        }

        private void HandleTargetChanged()
        {
            RefreshProp();

            // Passes along the event from Target -> Logic -> Listener
            OnTargetChanged?.Invoke(_target);
        }

        private void HandleHotbarSelectedSlotChanged(HotbarSlot slot)
        {
            RefreshProp();
        }

        public override void Tick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            DetermineTargetTick();
            TransformPropTick();
        }

        private void DetermineTargetTick()
        {
            Vector2Int newTargetCell;

            if (!_isBuilding)
            {
                Vector2Int playerCell = _context.Raft.Queries.WorldPositionToCell(_player.transform.position);

                List<RaftTile> tiles = ListPool<RaftTile>.Get();

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (_context.Raft.Tiles.TryGetValue(playerCell + new Vector2Int(i, j), out RaftTile tile))
                        {
                            tiles.Add(tile);
                        }
                    }
                }

                // Find the closest tile that can be repaired
                RaftTile closestTile = tiles
                    .Where(tile => tile.EntityHealthLogic.Current < tile.EntityHealthLogic.Max && Vector3.Distance(tile.transform.position, _player.transform.position) < RepairRange)
                    .OrderBy(tile => Vector3.Distance(tile.transform.position, _player.transform.position))
                    .FirstOrDefault();

                ListPool<RaftTile>.Release(tiles);

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

        private void TransformPropTick()
        {
            if (_targetProp == null)
            {
                return;
            }

            Vector3 position = _context.Raft.Queries.CellToWorldPosition(_target.Cell);
            
            if (_target.Tile != null)
            {
                position.y = _target.Tile.GetSurfaceY();
            }

            if (_targetProp.Id == PropId.TileScaffold)
            {
                position.y = -0.125f;
            }

            _targetProp.transform.position = position;
        }

        private void RefreshProp()
        {
            PropId id = PropId.None;
            Color color = Color.white;

            if (_isBuilding)
            {
                if (_target.CanBuildTile())
                {
                    id = PropId.TileScaffold;
                    color = _settings.ValidColor;
                }
                else
                {
                    id = PropId.StructureScaffold;
                    color = _target.CanBuildStructure() ? _settings.ValidColor : _settings.InvalidColor;
                }
            }

            if (_targetProp != null && _targetProp.Id != id)
            {
                _environmentManager.ReturnProp(_targetProp);
                _targetProp = null;
            }

            if (_targetProp == null && id != PropId.None)
            {
                _targetProp = _environmentManager.GetProp(id, new SpawnParams());
            }

            _targetProp?.SetColor(color);
        }

        public void SetIsBuilding(bool building)
        {
            if (_isBuilding == building)
            {
                return;
            }

            _isBuilding = building;

            RefreshProp();
        }
    }
}