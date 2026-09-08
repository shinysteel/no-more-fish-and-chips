using NoMoreFishAndChips.Entities;
using ShinyOwl.Common;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Utils;
using System.Linq;

namespace NoMoreFishAndChips.Environments
{
    // A RaftAxis will be either horizontal or vertical. It contains a collection of RaftLines, and keeps track of its bounds
    public class RaftAxis
    {
        private Raft _raft;
        private Axis _type;
        private SortedDictionary<int, RaftLine> _lines = new();
        private IntRange _linesBounds;

        public Axis Type => _type;
        public IReadOnlyDictionary<int, RaftLine> Lines => _lines;

        public RaftAxis(Raft raft, Axis type)
        {
            _raft = raft;
            _type = type;

            _raft.OnTileChanged += HandleTileChanged;
        }

        public void Dispose()
        {
            if (_raft != null)
            {
                _raft.OnTileChanged -= HandleTileChanged;
            }
        }

        private void HandleTileChanged(Vector2Int cell, RaftTile previous, RaftTile current)
        {
            UpdateLines(cell, current);
        }

        // Maintains positional maps when any Tile is changed
        private void UpdateLines(Vector2Int cell, RaftTile tile)
        {
            int lineIndex = CellToLineIndex(cell);
            int axisIndex = CellToAxisIndex(cell);

            if (tile != null)
            {
                if (!_lines.ContainsKey(lineIndex))
                {
                    _lines.Add(lineIndex, new RaftLine(this, lineIndex));
                    RefreshLinesBounds();
                }

                _lines[lineIndex].AddNode(axisIndex);
            }
            else
            {
                _lines[lineIndex].RemoveNode(axisIndex);

                if (_lines[lineIndex].Nodes.Count == 0)
                {
                    _lines.Remove(lineIndex);
                    RefreshLinesBounds();
                }
            }
        }

        private void RefreshLinesBounds()
        {
            _linesBounds = _lines.Count > 0
                ? new IntRange(_lines.Keys.Min(), _lines.Keys.Max())
                : null;
        }

        public bool TryGetLinesBounds(out IntRange bounds)
        {
            bounds = _linesBounds;
            return bounds != null;
        }

        public int CellToLineIndex(Vector2Int cell)
        {
            return _type == Axis.Horizontal ? cell.y : cell.x;
        }

        // The AxisIndex is a stripped coordinate from a position - one that is relevant to this axis. For example, if this axis is Horizontal, the x-coordinate is relevant
        public int CellToAxisIndex(Vector2Int cell)
        {
            return _type == Axis.Horizontal ? cell.x : cell.y;
        }

        public int WorldPositionToAxisIndex(Vector3 position)
        {
            return CellToAxisIndex(_raft.Queries.WorldPositionToCell(position));
        }

        public Direction GetDirection()
        {
            return _type == Axis.Horizontal ? Direction.Up : Direction.Right;
        }
    }
}