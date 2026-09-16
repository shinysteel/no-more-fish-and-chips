using NoMoreFishAndChips.States;
using UnityEngine;
using System;
using NoMoreFishAndChips.Networking;
using ShinyOwl.Common.Utils;
using ShinyOwl.Common;
using NoMoreFishAndChips.Environments;

namespace NoMoreFishAndChips.Entities
{
    public class TileBuildTarget : BuildTarget, IStateManagerListener
    {
        private StateManager _stateManager;
        private EntityManager _entityManager;

        private RaftTile _tile;
        private EntityModel _previewModel;

        public TileBuildTarget(GameplayContext context, BuildTargetSettings settings, EntityId entityId) : base(context, settings, entityId)
        {
            _stateManager = GameManager.Instance.Get<StateManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();

            _previewModel = _entityManager.GetModel(_entityId, new SpawnParams() { Rotation = Quaternion.LookRotation(Vector3.back, Vector3.up), Scale = Vector3.one * 1.01f });

            RefreshPreview();

            _stateManager.AddListener(this);

            _context.Raft.OnTileChanged += HandleTileChanged;
        }

        public override void Dispose()
        {
            _entityManager.ReturnModel(_previewModel);

            _stateManager.RemoveListener(this);

            if (_context.Raft != null)
            {
                _context.Raft.OnTileChanged -= HandleTileChanged;
            }
        }

        public override void SetPosition(Vector3 position)
        {            
            if (_position == position)
            {
                return;
            }

            Vector2Int cell = _context.Raft.Queries.WorldPositionToTileCell(position);

            if (_cell == cell)
            {
                return;
            }

            _cell = cell;
            
            _context.Raft.Tiles.TryGetValue(_cell, out RaftTile tile);

            HandleTileChanged(_cell, null, tile);
        }

        private void HandleTileChanged(Vector2Int cell, RaftTile previous, RaftTile current)
        {
            if (_cell != cell)
            {
                return;
            }

            _tile = current;

            RefreshPreview();
        }

        private void RefreshPreview()
        {
            Vector3 position = _context.Raft.Queries.TileCellToWorldPosition(_cell);
            position.y = -0.125f;
            _previewModel.transform.position = position;
            _previewModel.SetColor(CanBuild() ? _settings.ValidColor : _settings.InvalidColor);            
        }

        public override void Tick()
        {
            PreviewTick();
        }

        private void PreviewTick()
        {
            float y = _tile != null ? _tile.transform.position.y - 0.25f : -0.125f;

            Vector3 position = _previewModel.transform.position;
            position.y = y;

            _previewModel.transform.position = position;
        }

        protected override bool CanBuild()
        {
            if (_tile != null)
            {
                return false;
            }

            if (_stateManager.CurrentStatePath.Contains(EGameplayState.Stage))
            {
                return true;
            }

            return _context.Raft.Queries.Axes[Axis.Vertical].TryGetLinesBounds(out IntRange range) && _cell.x >= range.Min;
        }

        void IStateManagerListener.OnStatePathChanged(StatePath previous, StatePath current)
        {
            if (previous.Contains(EGameplayState.Stage) != current.Contains(EGameplayState.Stage))
            {
                RefreshPreview();
            }
        }
    }
}