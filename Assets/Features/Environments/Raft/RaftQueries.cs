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
    /// A RaftAxis will be either horizontal or vertical. It contains a collection of RaftLines, and keeps
    /// track of its bounds
    /// </summary>
    public class RaftAxis
    {
        private Raft _raft;
        private Axis _type;
        private Dictionary<int, RaftLine> _lines = new();

        public Axis Type => _type;

        private int _minLineIndex;
        private int _maxLineIndex;

        public int MinLineIndex => _minLineIndex;
        public int MaxLineIndex => _maxLineIndex;

        public IReadOnlyDictionary<int, RaftLine> Lines => _lines;

        // An arbitrary value for how many lines we setup on each axix
        public const int DefaultLines = 20;

        public RaftAxis(Raft raft, Axis type)
        {
            _raft = raft;
            _type = type;

            for (int i = 0; i < DefaultLines; i++)
            {
                // Evenly distribute from negative to positive
                int lineIndex = i - DefaultLines / 2;
                _lines.Add(lineIndex, new RaftLine(this, lineIndex));
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
            int lineIndex = CellToLineIndex(cell);
            int axisIndex = CellToAxisIndex(cell);

            if (!_lines.ContainsKey(lineIndex))
            {
                Log.Error($"No line exists for the cell {cell}");
                return;
            }

            if (tile == null)
            {
                _lines[lineIndex].RemoveNode(axisIndex);
            } 
            else if (!_lines[lineIndex].Nodes.ContainsKey(axisIndex))
            {
                _lines[lineIndex].AddNode(axisIndex, new RaftLineNode(axisIndex, _lines[lineIndex]));
            }
        }

        // Recalculates boundaries when any Tile is changed
        private void UpdateBounds(Vector2Int cell, Tile tile)
        {
            int lineIndex = CellToLineIndex(cell);

            if (tile != null)
            {
                _minLineIndex = Mathf.Min(_minLineIndex, lineIndex);
                _maxLineIndex = Mathf.Max(_maxLineIndex, lineIndex);
            }
            else
            {
                if (_minLineIndex == lineIndex && !_lines.ContainsKey(lineIndex))
                {
                    _minLineIndex = _lines.Keys.Min();
                }

                if (_maxLineIndex == lineIndex && !_lines.ContainsKey(lineIndex))
                {
                    _maxLineIndex = _lines.Keys.Max();
                }
            }
        }

        public int CellToLineIndex(Vector2Int cell)
        {
            return _type == Axis.Horizontal ? cell.y : cell.x;
        }

        // The AxisIndex is a stripped coordinate from a position - one that is relevant to this axis. For example,
        // if this axis is Horizontal, the x-coordinate is relevant
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

    /// <summary>
    /// A RaftLine represents a span of cells along an index on an axis. It stores its ends
    /// into the values '_minEdge' and '_maxEdge'
    /// </summary>
    public class RaftLine
    {
        private RaftAxis _raftAxis;
        public RaftAxis RaftAxis => _raftAxis;

        private int _lineIndex;
        public int LineIndex => _lineIndex;

        private SortedDictionary<int, RaftLineNode> _nodes;
        public IReadOnlyDictionary<int, RaftLineNode> Nodes => _nodes;

        private RaftEdge _minEdge;
        private RaftEdge _maxEdge;

        public RaftEdge MinEdge => _minEdge;
        public RaftEdge MaxEdge => _maxEdge;

        public RaftLine(RaftAxis raftAxis, int lineIndex)
        {
            _raftAxis = raftAxis;
            _lineIndex = lineIndex;

            _nodes = new();
        }

        public void AddNode(int axisIndex, RaftLineNode node)
        {
            _nodes.Add(axisIndex, node);

            RefreshEdges();
        }

        public void RemoveNode(int axisIndex)
        {
            _nodes.Remove(axisIndex);

            RefreshEdges();
        }

        // Manual refresh when we know min and max are potentially dirty
        private void RefreshEdges()
        {
            if (_nodes.Count == 0)
            {
                _minEdge = null;
                _maxEdge = null;
                return;
            }

            RaftLineNode minNode = _nodes.First().Value;
            RaftLineNode maxNode = _nodes.Last().Value;

            _minEdge = new RaftEdge(minNode, _raftAxis.Type == Axis.Horizontal ? Direction.Left : Direction.Down);
            _maxEdge = new RaftEdge(maxNode, _raftAxis.Type == Axis.Horizontal ? Direction.Right : Direction.Up);
        }

        public RaftEdge GetEdge(int direction)
        {
            return direction < 0 ? _minEdge : _maxEdge;
        }

        public RaftEdge GetRandomEdge()
        {
            return Random.value < 0.5f ? _minEdge : _maxEdge;
        }

        public RaftLineNode GetNextNode(int axisIndex, int direction)
        {
            if (direction < 0)
            {
                return _nodes.Values.LastOrDefault(node => node.AxisIndex < axisIndex);
            }
            else
            {
                return _nodes.Values.FirstOrDefault(node => node.AxisIndex > axisIndex);
            }
        }

        public Vector2Int AxisIndexToCell(int axisIndex)
        {
            if (_raftAxis.Type == Axis.Horizontal)
            {
                return new Vector2Int(axisIndex, _lineIndex);
            }
            else
            {
                return new Vector2Int(_lineIndex, axisIndex);
            }
        }

        public Vector3 AxisIndexToWorldPosition(int axisIndex)
        {
            if (_raftAxis.Type == Axis.Horizontal)
            {
                return new Vector3(axisIndex, 0f, _lineIndex);
            }
            else
            {
                return new Vector3(_lineIndex, 0f, axisIndex);
            }
        }
    }

    /// <summary>
    /// A context container for both a Tile and it's AxisIndex within a RaftLine
    /// </summary>
    public class RaftLineNode : IComparable<RaftLineNode>
    {
        private int _axisIndex;
        public int AxisIndex => _axisIndex;

        private RaftLine _line;

        private Vector2Int _cell;
        public Vector2Int Cell => _cell;

        public RaftLineNode(int axisIndex, RaftLine line)
        {
            _axisIndex = axisIndex;
            _line = line;
            _cell = _line.AxisIndexToCell(_axisIndex);
        }

        public int CompareTo(RaftLineNode other)
        {
            return _axisIndex.CompareTo(other._axisIndex);
        }
    }

    /// <summary>
    /// A RaftEdge captures information about a tile that at the time of creation was considered on
    /// the edge of the raft. This simply means that the tile had either the smallest or biggest value in a RaftLine
    /// </summary>
    public class RaftEdge
    {
        private RaftLineNode _node;
        private Direction _direction;

        public RaftLineNode Node => _node;
        public Direction Direction => _direction;

        public RaftEdge(RaftLineNode node, Direction direction)
        {
            _node = node;
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

        public bool TryGetClosestTile(Vector3 position, out Tile tile)
        {
            tile = null;

            if (_raft.Tiles.Count == 0)
            {
                return false;
            }

            tile = _raft.Tiles.Values.OrderBy(tile => Vector3.Distance(tile.transform.position, position)).First();

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
                    if (targetLine.RaftAxis.Lines.TryGetValue(targetLine.LineIndex + i, out RaftLine line))
                    {
                        lines.Add(line);
                    }
                }

                if (lines.Count == 0)
                {
                    return false;
                }

                adjacentLine = lines[Random.Range(0, lines.Count)];
                adjacentDirection = adjacentLine.LineIndex < targetLine.LineIndex ? -1 : 1;

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

            if (horizontalLine.Nodes.Count == 0 || verticalLine.Nodes.Count == 0)
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

            int minDistance = edges.Min(edge => (cell - edge.Node.Cell).sqrMagnitude);

            closestEdge = edges
                .Where(edge => (cell - edge.Node.Cell).sqrMagnitude == minDistance)
                .OrderBy(_ => Random.value)
                .First();

            return true;
        }
    }
}