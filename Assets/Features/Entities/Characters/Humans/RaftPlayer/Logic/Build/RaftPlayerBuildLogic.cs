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
    public class RaftPlayerBuildLogic : RaftPlayerLogic
    {
        private CameraManager _cameraManager;
        private EnvironmentManager _environmentManager;
        private EntityManager _entityManager;

        private RaftPlayerBuildSettings _settings;

        private BuildTarget _buildTarget;

        public bool IsBuilding => _buildTarget != null;

        public RaftPlayerBuildLogic(RaftPlayer player) : base(player)
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _environmentManager = GameManager.Instance.Get<EnvironmentManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();

            _settings = _player.DefinitionData.BuildSettings;
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
                // RefreshProp();
            }
        }

        public void SetBuildTarget(EntityId buildableId)
        {
            Entity entity = _entityManager.GetPrefab(buildableId);

            _buildTarget?.Dispose();

            _buildTarget = entity switch
            {
                RaftTile => new TileBuildTarget(_context, _settings.TargetSettings, buildableId),
                Structure => new StructureBuildTarget(_context, _settings.TargetSettings, buildableId),
                _ => null
            };

            _buildTarget?.SetPosition(_player.transform.position + _player.transform.forward * 0.75f);

            _player.ContextLogic.SetContext(_buildTarget != null ? _settings.ActionDatas : null);
        }

        public override void Tick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            _buildTarget?.SetPosition(_player.transform.position + _player.transform.forward * 0.75f);
            _buildTarget?.Tick();            
        }
    }
}