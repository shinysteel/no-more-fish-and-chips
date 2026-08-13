using NoMoreFishAndChips.States;
using UnityEngine;
using System;
using NoMoreFishAndChips.Networking;
using ShinyOwl.Common.Utils;
using ShinyOwl.Common;

namespace NoMoreFishAndChips.Entities
{
    public class RaftTileTarget : IStateManagerListener
    {
        private StateManager _stateManager;

        private GameplayContext _context;

        private Vector2Int _cell;
        private RaftTile _tile;

        public Vector2Int Cell => _cell;
        public RaftTile Tile => _tile;

        public event Action OnChanged;

        public RaftTileTarget(GameplayContext context)
        {
            _stateManager = GameManager.Instance.Get<StateManager>();

            _stateManager.AddListener(this);

            _context = context;

            _cell = Vector2Int.one * int.MinValue;

            _context.Raft.OnTileChanged += HandleTileChanged;
        }

        public void Dispose()
        {
            _stateManager?.RemoveListener(this);

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

            OnChanged?.Invoke();
        }

        public bool CanBuild()
        {
            return CanBuildTile() || CanBuildStructure();
        }

        public bool CanBuildTile()
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

        public bool CanBuildStructure()
        {
            return _tile != null && _tile.Structure == null;
        }

        public bool CanRepair()
        {
            return _tile?.EntityHealthLogic.Current < _tile?.EntityHealthLogic.Max;
        }

        void IStateManagerListener.OnStatePathChanged(StatePath previous, StatePath current)
        {
            if (previous.Contains(EGameplayState.Stage) != current.Contains(EGameplayState.Stage))
            {
                OnChanged?.Invoke();
            }
        }
    }
}