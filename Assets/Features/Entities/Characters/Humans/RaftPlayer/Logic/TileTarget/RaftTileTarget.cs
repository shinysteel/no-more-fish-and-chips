using NoMoreFishAndChips.States;
using UnityEngine;
using System;

namespace NoMoreFishAndChips.Entities
{
    public class RaftTileTarget
    {
        private GameplayContext _context;

        private Vector2Int _cell;
        private RaftTile _tile;

        public Vector2Int Cell => _cell;
        public RaftTile Tile => _tile;

        public event Action OnChanged;

        public RaftTileTarget(GameplayContext context)
        {
            _context = context;

            _cell = Vector2Int.one * int.MinValue;

            _context.Raft.OnTileChanged += HandleTileChanged;
        }

        ~RaftTileTarget()
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
            _context.Raft.Tiles.TryGetValue(_cell, out RaftTile tile);
            HandleTileChanged(_cell, tile);
        }

        private void HandleTileChanged(Vector2Int cell, RaftTile tile)
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
            return _tile?.EntityHealthLogic.Current < _tile?.EntityHealthLogic.Max;
        }
    }
}