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
        private EnvironmentManager _environmentManager;

        private RaftTile _tile;
        private Prop _prop;

        public TileBuildTarget(GameplayContext context, BuildTargetSettings settings, Vector3 position) : base(context, settings, position)
        {
            _stateManager = GameManager.Instance.Get<StateManager>();
            _environmentManager = GameManager.Instance.Get<EnvironmentManager>();

            _prop = _environmentManager.GetProp(PropId.TileScaffold, new SpawnParams());

            RefreshProp();

            _stateManager.AddListener(this);

            _context.Raft.OnTileChanged += HandleTileChanged;
        }

        public override void Dispose()
        {
            _environmentManager.ReturnProp(_prop);

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

            RefreshProp();
        }

        private void RefreshProp()
        {
            if (_prop == null)
            {
                return;
            }

            Vector3 position = _context.Raft.Queries.TileCellToWorldPosition(_cell);
            position.y = -0.125f;
            _prop.transform.position = position;
            _prop.SetColor(CanBuild() ? _settings.ValidColor : _settings.InvalidColor);            
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
                RefreshProp();
            }
        }
    }
}