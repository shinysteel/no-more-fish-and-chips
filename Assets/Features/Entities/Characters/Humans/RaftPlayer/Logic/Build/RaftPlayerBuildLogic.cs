using NoMoreFishAndChips.Cameras;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Instantiating;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.States;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerBuildLogic : RaftPlayerLogic
    {
        private EntityManager _entityManager;

        private RaftPlayerBuildSharedSettings _sharedSettings;

        private BuildTarget _buildTarget;

        public bool IsBuilding => _buildTarget != null;

        public RaftPlayerBuildLogic(RaftPlayer player) : base(player)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();

            _sharedSettings = _player.DefinitionData.BuildSharedSettings;
        }

        public override void Dispose()
        {
            _buildTarget?.Dispose();
        }

        public void SetBuildTarget(EntityId buildableId)
        {
            Entity entity = _entityManager.GetPrefab(buildableId);

            _buildTarget?.Dispose();

            _buildTarget = entity switch
            {
                RaftTile => new TileBuildTarget(_context, _sharedSettings.TargetSettings, buildableId),
                Structure => new StructureBuildTarget(_context, _sharedSettings.TargetSettings, buildableId),
                _ => null
            };

            if (entity is Structure)
            {
                Vector3 direction = _player.transform.forward;
                direction.y = 0f;
                direction.Normalize();

                float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                int rotations = Utils.Math.EuclideanModulo(Mathf.RoundToInt(angle / 90f), 4);

                _buildTarget.SetRotations(rotations);
            }

            _buildTarget?.SetPosition(_player.transform.position + _player.transform.forward * _sharedSettings.Range);

            _player.ContextActionsLogic.SetActionDatas(_buildTarget != null ? _sharedSettings.ActionDatas : null);
        }

        public override void Tick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            _buildTarget?.SetPosition(_player.transform.position + _player.transform.forward * _sharedSettings.Range);
            _buildTarget?.Tick();            
        }

        public void Rotate()
        {
            _buildTarget?.ChangeRotations(1);   
        }

        public void Place()
        {
            _buildTarget?.Place();
        }
    }
}