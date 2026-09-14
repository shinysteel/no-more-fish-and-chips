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

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerBuildTargetLogic : RaftPlayerLogic
    {
        private CameraManager _cameraManager;
        private EnvironmentManager _environmentManager;
        private EntityManager _entityManager;

        private RaftPlayerBuildTargetSettings _settings;

        private BuildTarget _buildTarget;

        public bool IsBuilding => _buildTarget != null;

        public RaftPlayerBuildTargetLogic(RaftPlayer player) : base(player)
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _environmentManager = GameManager.Instance.Get<EnvironmentManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();

            _settings = _player.DefinitionData.TileTargetSettings;
        }

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            _player.Hotbar.OnSelectedChanged += HandleHotbarSelectedSlotChanged;
        }

        public override void Dispose()
        {
            _buildTarget?.Dispose();
        }

        public override void OnDespawned()
        {
            if (_player != null)
            {
                _player.Hotbar.OnSelectedChanged -= HandleHotbarSelectedSlotChanged;
            }
        }

        private void HandleHotbarSelectedSlotChanged(HotbarSlot slot)
        {
            if (_player.isOwner)
            {
                RefreshProp();
            }
        }

        public void SetBuildTarget(EntityId buildableId)
        {
            Entity entity = _entityManager.GetPrefab(buildableId);
            Vector3 position = _player.transform.position + _player.transform.forward * 1f;

            if (entity is RaftTile tile)
            {
                _buildTarget = new TileBuildTarget(_context, _settings, position);
            }
            else if (entity is Structure structure)
            {
                _buildTarget = new StructureBuildTarget(_context, _settings, position, buildableId);
            }
            else
            {
                _buildTarget?.Dispose();
                _buildTarget = null;
            }
        }

        public override void Tick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            if (_buildTarget == null)
            {
                return;
            }

            Vector3 position = _player.transform.position + _player.transform.forward * 1f;
            _buildTarget.SetPosition(position);
        }

        private void TransformPropTick()
        {
            //if (_targetProp == null)
            //{
            //    return;
            //}

            //Vector3 position = _context.Raft.Queries.CellToWorldPosition(_target.Cell);

            //if (_targetProp.Id == PropId.TileScaffold)
            //{
            //    position.y = -0.125f;
            //}

            //_targetProp.transform.position = position;
        }

        private void RefreshProp()
        {
            //PropId id = PropId.None;
            //Color color = Color.white;

            //if (_isBuilding)
            //{
            //    if (_target.Tile != null)
            //    {
            //        id = PropId.StructureScaffold;
            //        color = _target.CanBuildStructure() ? _settings.ValidColor : _settings.InvalidColor;
            //    }
            //    else
            //    {
            //        id = PropId.TileScaffold;
            //        color = _target.CanBuildTile() ? _settings.ValidColor : _settings.InvalidColor;
            //    }
            //}

            //if (_targetProp != null && _targetProp.Id != id)
            //{
            //    _environmentManager.ReturnProp(_targetProp);
            //    _targetProp = null;
            //}

            //if (_targetProp == null && id != PropId.None)
            //{
            //    _targetProp = _environmentManager.GetProp(id, new SpawnParams());
            //}

            //_targetProp?.SetColor(color);
        }
    }
}