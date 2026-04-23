using FishFlingers.Entities;
using FishFlingers.Networking;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace FishFlingers.Environments
{
    /// <summary>
    /// A RaftLine will be either horizontal or vertical. It contains a collection of RaftLines, and keeps
    /// track of its bounds
    /// </summary>
    public class RaftAxis
    {
        private Raft _raft;
        private Axis _type;
        private Dictionary<int, RaftLine> _lines = new();

        public Axis Type => _type;

        private int _minIndex;
        private int _maxIndex;

        public int MinIndex => _minIndex;
        public int MaxIndex => _maxIndex;

        public IReadOnlyDictionary<int, RaftLine> Lines => _lines;

        // An arbitrary value for how many lines we setup on each axix
        private const int DefaultLines = 20;

        public RaftAxis(Raft raft, Axis type)
        {
            _raft = raft;
            _type = type;

            for (int i = 0; i < DefaultLines; i++)
            {
                // Evenly distribute from negative to positive
                int index = i - DefaultLines / 2;
                _lines.Add(index, new RaftLine(this, index));
            }
            
            _raft.OnTileChanged += HandleTileChanged;
        }

        ~RaftAxis()
        {
            if (_raft != null)
            {
                _raft.OnTileChanged -= HandleTileChanged;
            }
        }

        private void HandleTileChanged(Vector2Int cell, Tile tile)
        {
            UpdateLines(cell, tile);
            UpdateBounds(cell, tile);
        }

        // Maintains positional maps when any Tile is changed
        private void UpdateLines(Vector2Int cell, Tile tile)
        {
            int index = _type == Axis.Horizontal ? cell.y : cell.x;

            if (!_lines.ContainsKey(index))
            {
                Log.Error($"No line exists for the cell {cell}");
                return;
            }

            if (tile != null)
            {
                _lines[index].AddTile(tile);
            }
            else
            {
                _lines[index].RemoveTile(cell);
            }
        }

        // Recalculates boundaries when any Tile is changed
        private void UpdateBounds(Vector2Int cell, Tile tile)
        {
            int index = _type == Axis.Horizontal ? cell.y : cell.x;

            if (tile != null)
            {
                _minIndex = Mathf.Min(_minIndex, index);
                _maxIndex = Mathf.Max(_maxIndex, index);
            }
            else
            {
                if (_minIndex == index && !_lines.ContainsKey(index))
                {
                    _minIndex = _lines.Keys.Min();
                }

                if (_maxIndex == index && !_lines.ContainsKey(index))
                {
                    _maxIndex = _lines.Keys.Max();
                }
            }
        }

        // The AxisIndex is a stripped coordinate from a position - one that is relevant to this axis. For example,
        // if this axis is Horizontal, the x-coordinate is relevant
        public int GetAxisIndex(Vector2Int cell)
        {
            return _type == Axis.Horizontal ? Mathf.RoundToInt(cell.x) : Mathf.RoundToInt(cell.y);
        }

        public int GetAxisIndex(Vector3 position)
        {
            return GetAxisIndex(new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z)));
        }
    }

    /// <summary>
    /// A context container for both a Tile and it's AxisIndex within a RaftLine
    /// </summary>
    public class RaftLineTile : IComparable<RaftLineTile>
    {
        private Tile _tile;
        private RaftLine _line;
        private int _axisIndex;

        public Tile Tile => _tile;
        public int AxisIndex => _axisIndex;

        public RaftLineTile(Tile tile, RaftLine line)
        {
            _tile = tile;
            _line = line;
            _axisIndex = _line.RaftAxis.GetAxisIndex(_tile.Cell);
        }

        public int CompareTo(RaftLineTile other)
        {
            return _axisIndex.CompareTo(other._axisIndex);
        }
    }

    /// <summary>
    /// A RaftLine represents a span of cells along an index on an axis. It stores its ends
    /// into the values '_minEdge' and '_maxEdge'
    /// </summary>
    public class RaftLine
    {
        private RaftAxis _raftAxis;
        public RaftAxis RaftAxis => _raftAxis;

        private int _index;
        public int Index => _index;

        private SortedSet<RaftLineTile> _lineTiles;
        public IReadOnlyCollection<RaftLineTile> LineTiles => _lineTiles;

        private RaftEdge _minEdge;
        private RaftEdge _maxEdge;

        public RaftEdge MinEdge => _minEdge;
        public RaftEdge MaxEdge => _maxEdge;

        public RaftLine(RaftAxis axis, int index)
        {
            _raftAxis = axis;
            _index = index;

            _lineTiles = new();
        }

        public void AddTile(Tile tile)
        {
            _lineTiles.Add(new RaftLineTile(tile, this));

            RefreshEdges();
        }

        public void RemoveTile(Vector2Int cell)
        {
            _lineTiles.RemoveWhere(tile => tile.Tile.Cell == cell);

            RefreshEdges();
        }

        // Manual refresh when we know min and max are potentially dirty
        private void RefreshEdges()
        {
            if (_lineTiles.Count == 0)
            {
                _minEdge = null;
                _maxEdge = null;
                return;
            }

            RaftLineTile minLineTile = _lineTiles.First();
            RaftLineTile maxLineTile = _lineTiles.Last();

            _minEdge = new RaftEdge(minLineTile, _raftAxis.Type == Axis.Horizontal ? Direction.Left : Direction.Down);
            _maxEdge = new RaftEdge(maxLineTile, _raftAxis.Type == Axis.Horizontal ? Direction.Right : Direction.Up);
        }

        public RaftEdge GetEdge(int direction)
        {
            return direction < 0 ? _minEdge : _maxEdge;
        }

        public RaftEdge GetRandomEdge()
        {
            return Random.value < 0.5f ? _minEdge : _maxEdge;
        }

        public RaftLineTile GetNextLineTile(int axisIndex, int direction)
        {
            if (direction < 0)
            {
                return _lineTiles.LastOrDefault(lineTile => lineTile.AxisIndex < axisIndex);
            }
            else
            {
                return _lineTiles.FirstOrDefault(lineTile => lineTile.AxisIndex > axisIndex);
            }
        }
    }

    /// <summary>
    /// A RaftEdge captures information about a tile that at the time of creation was considered on
    /// the edge of the raft. This simply means that the tile had either the smallest or biggest value in a RaftLine
    /// </summary>
    public class RaftEdge
    {
        private RaftLineTile _lineTile;
        private Direction _direction;

        public RaftLineTile LineTile => _lineTile;
        public Direction Direction => _direction;

        public RaftEdge(RaftLineTile lineTile, Direction direction)
        {
            _lineTile = lineTile;
            _direction = direction;
        }
    }

    public class RaftQueries
    {
        private Raft _raft;

        // Axes is plural for axis
        private Dictionary<Axis, RaftAxis> _axes = new();

        public IReadOnlyDictionary<Axis, RaftAxis> Axes => _axes;

        public RaftQueries(Raft raft)
        {
            _raft = raft;

            _axes.Add(Axis.Horizontal, new RaftAxis(_raft, Axis.Horizontal));
            _axes.Add(Axis.Vertical, new RaftAxis(_raft, Axis.Vertical));
        }

        // Uses Vector2 to allow for floating-point cells
        public Vector3 CellToWorldPosition(Vector2 cell)
        {
            return new Vector3(cell.x, 0f, cell.y);
        }

        public Vector2Int WorldPositionToCell(Vector3 position)
        {
            return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        }

        /// <summary>
        /// Retrieves a random tile
        /// </summary>
        public bool TryGetRandomTile(out Tile tile)
        {
            tile = null;

            if (_raft.Tiles.Count == 0)
            {
                return false;
            }

            tile = _raft.Tiles.Values.ElementAt(Random.Range(0, _raft.Tiles.Count));

            return true;
        }

        /// <summary>
        /// Retrieves a random line that fulfills a coniditon
        /// </summary>
        public bool TryGetRandomLine(Func<RaftLine, bool> condition, out RaftLine randomLine)
        {
            List<RaftLine> lines = ListPool<RaftLine>.Get();
            List<RaftLine> candidates = ListPool<RaftLine>.Get();

            randomLine = null;

            foreach (RaftAxis axis in _axes.Values)
            {
                lines.AddRange(axis.Lines.Values);
            }

            foreach (RaftLine line in lines)
            {
                if (condition(line))
                {
                    candidates.Add(line);   
                }
            }

            try
            {
                if (candidates.Count == 0)
                {
                    return false;
                }

                randomLine = candidates[Random.Range(0, candidates.Count)];

                return true;
            }
            finally
            {
                ListPool<RaftLine>.Release(lines);
                ListPool<RaftLine>.Release(candidates);
            }
        }

        public bool TryGetRandomAdjacentLine(RaftLine targetLine, out RaftLine adjacentLine, out int adjacentDirection)
        {
            adjacentLine = null;
            adjacentDirection = 0;

            List<RaftLine> lines = ListPool<RaftLine>.Get();

            try
            {
                // An index of +1 or -1 means the line is adjacent
                for (int i = -1; i < 2; i += 2)
                {
                    if (targetLine.RaftAxis.Lines.TryGetValue(targetLine.Index + i, out RaftLine line))
                    {
                        lines.Add(line);
                    }
                }

                if (lines.Count == 0)
                {
                    return false;
                }

                adjacentLine = lines[Random.Range(0, lines.Count)];
                adjacentDirection = adjacentLine.Index < targetLine.Index ? -1 : 1;

                return true;
            }
            finally
            {
                ListPool<RaftLine>.Release(lines);
            }
        }

        /// <summary>
        /// Finds the closest edge to a cell. Ties are resolved randomly
        /// </summary>
        public bool TryGetClosestEdge(Vector2Int cell, out RaftEdge closestEdge)
        {
            closestEdge = null;

            if (!_axes[Axis.Horizontal].Lines.TryGetValue(cell.y, out RaftLine horizontalLine) 
                || !_axes[Axis.Vertical].Lines.TryGetValue(cell.x, out RaftLine verticalLine))
            {
                return false;
            }

            if (horizontalLine.LineTiles.Count == 0 || verticalLine.LineTiles.Count == 0)
            {
                return false;
            }

            RaftEdge[] edges = new RaftEdge[]
            {
                horizontalLine.MinEdge,
                horizontalLine.MaxEdge,
                verticalLine.MinEdge,
                verticalLine.MaxEdge
            };

            int minDistance = edges.Min(edge => (cell - edge.LineTile.Tile.Cell).sqrMagnitude);

            closestEdge = edges
                .Where(edge => (cell - edge.LineTile.Tile.Cell).sqrMagnitude == minDistance)
                .OrderBy(_ => Random.value)
                .First();

            return true;
        }
    }
}