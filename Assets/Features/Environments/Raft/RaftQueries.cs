using ShinyOwl.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FishFlingers.Entities;
using FishFlingers.Networking;
using System.Collections;
using UnityEngine.Pool;
using System.Buffers;

namespace FishFlingers.Environments
{
    public enum ERaftAxis
    {
        Horizontal,
        Vertical
    }

    public class RaftAxis
    {
        private Raft _raft;
        private ERaftAxis _type;
        private Dictionary<int, RaftLine> _lines = new();

        private int _minAnchor;
        private int _maxAnchor;

        public int MinAnchor => _minAnchor;
        public int MaxAnchor => _maxAnchor;

        public IReadOnlyDictionary<int, RaftLine> Lines => _lines;

        private const int DefaultLines = 20;

        public RaftAxis(Raft raft, ERaftAxis type)
        {
            _raft = raft;
            _type = type;

            for (int i = 0; i < DefaultLines; i++)
            {
                int anchor = i - DefaultLines / 2;
                _lines.Add(anchor, new RaftLine(_type));
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

        // Maintains positional maps when SetTile is called
        private void UpdateLines(Vector2Int cell, Tile tile)
        {
            int anchor = _type == ERaftAxis.Horizontal ? cell.y : cell.x;
            int index = _type == ERaftAxis.Horizontal ? cell.x : cell.y;

            if (!_lines.ContainsKey(anchor))
            {
                Log.Error($"No line exists for the cell {cell}");
                return;
            }

            if (tile != null)
            {
                _lines[anchor].AddTile(tile);
            }
            else
            {
                _lines[anchor].RemoveTile(cell);
            }
        }

        // Recalculates boundaries when SetTile is called
        private void UpdateBounds(Vector2Int cell, Tile tile)
        {
            int anchor = _type == ERaftAxis.Horizontal ? cell.y : cell.x;

            if (tile != null)
            {
                _minAnchor = Mathf.Min(_minAnchor, anchor);
                _maxAnchor = Mathf.Max(_maxAnchor, anchor);
            }
            else
            {
                if (_minAnchor == anchor && !_lines.ContainsKey(anchor))
                {
                    _minAnchor = _lines.Keys.Min();
                }

                if (_maxAnchor == anchor && !_lines.ContainsKey(anchor))
                {
                    _maxAnchor = _lines.Keys.Max();
                }
            }
        }
    }

    // A line represents a span of cells along a point and axis. They don't need tiles to exist
    public class RaftLine
    {
        private ERaftAxis _axis;

        private SortedSet<Tile> _tiles;
        public IReadOnlyCollection<Tile> Tiles => _tiles;

        private RaftEdge _minEdge;
        private RaftEdge _maxEdge;

        public RaftEdge MinEdge => _minEdge;
        public RaftEdge MaxEdge => _maxEdge;

        public RaftLine(ERaftAxis axis)
        {
            _axis = axis;

            _tiles = new SortedSet<Tile>(Comparer<Tile>.Create((a, b) =>
            {
                if (_axis == ERaftAxis.Horizontal)
                {
                    return a.Cell.x.CompareTo(b.Cell.x);
                }
                else
                {
                    return a.Cell.y.CompareTo(b.Cell.y);
                }
            }));
        }

        public void AddTile(Tile tile)
        {
            _tiles.Add(tile);

            RefreshEdges();
        }

        public void RemoveTile(Vector2Int cell)
        {
            _tiles.RemoveWhere(tile => tile.Cell == cell);

            RefreshEdges();
        }

        private void RefreshEdges()
        {
            if (_tiles.Count == 0)
            {
                _minEdge = null;
                _maxEdge = null;
                return;
            }
            
            _minEdge = new RaftEdge(_tiles.First(), _axis == ERaftAxis.Horizontal ? Vector2Int.left : Vector2Int.down);
            _maxEdge = new RaftEdge(_tiles.Last(), _axis == ERaftAxis.Horizontal ? Vector2Int.right : Vector2Int.up);
        }
    }

    public class RaftEdge
    {
        private Tile _tile;
        private Vector2Int _cellDirection;
        private Vector3 _worldDirection;

        public Tile Tile => _tile;
        public Vector2Int CellDirection => _cellDirection;
        public Vector3 WorldDirection => _worldDirection;

        public RaftEdge(Tile tile, Vector2Int cellDirection)
        {
            _tile = tile;
            _cellDirection = cellDirection;

            // Convert 2d to 3d
            if (_cellDirection == Vector2Int.up)
            {
                _worldDirection = Vector3.forward;
            }
            else if (_cellDirection == Vector2Int.right)
            {
                _worldDirection = Vector3.right;
            }
            else if (_cellDirection == Vector2Int.down)
            {
                _worldDirection = Vector3.back;
            }
            else if (_cellDirection == Vector2Int.left)
            {
                _worldDirection = Vector3.left;
            }
            else
            {
                _worldDirection = Vector3.zero;
            }
        }
    }

    public class RaftQueries
    {
        private Raft _raft;

        private Dictionary<ERaftAxis, RaftAxis> _axes = new();

        public IReadOnlyDictionary<ERaftAxis, RaftAxis> Axes => _axes;

        public RaftQueries(Raft raft)
        {
            _raft = raft;

            _axes.Add(ERaftAxis.Horizontal, new RaftAxis(_raft, ERaftAxis.Horizontal));
            _axes.Add(ERaftAxis.Vertical, new RaftAxis(_raft, ERaftAxis.Vertical));
        }

        // Uses Vector2 to allow for floating-point cells
        public Vector3 CellToWorldPosition(Vector2 cell)
        {
            return new Vector3(cell.x, 0f, cell.y);
        }

        public Vector2Int WorldPositionToCell(Vector3 position)
        {
            return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
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
        /// Retrieves a random line that has tiles in it
        /// </summary>
        public bool TryGetRandomLine(out RaftLine randomLine)
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
                if (line.Tiles.Count > 0)
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

        /// <summary>
        /// Finds the closest edge to a cell. Ties are resolved randomly
        /// </summary>
        public bool TryGetClosestEdge(Vector2Int cell, out RaftEdge closestEdge)
        {
            closestEdge = null;

            if (!_axes[ERaftAxis.Horizontal].Lines.TryGetValue(cell.y, out RaftLine horizontalLine) 
                || !_axes[ERaftAxis.Vertical].Lines.TryGetValue(cell.x, out RaftLine verticalLine))
            {
                return false;
            }

            if (horizontalLine.Tiles.Count == 0)
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

            int minDistance = edges.Min(edge => (cell - edge.Tile.Cell).sqrMagnitude);

            closestEdge = edges
                .Where(edge => (cell - edge.Tile.Cell).sqrMagnitude == minDistance)
                .OrderBy(_ => Random.value)
                .First();

            return true;
        }
    }
}