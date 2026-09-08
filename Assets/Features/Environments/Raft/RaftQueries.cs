using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
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

namespace NoMoreFishAndChips.Environments
{
    public class RaftQueries
    {
        private Raft _raft;

        // Axes is plural for axis
        private Dictionary<Axis, RaftAxis> _axes = new();

        public IReadOnlyDictionary<Axis, RaftAxis> Axes => _axes;

        private Dictionary<Vector2Int, RaftPerimeterCell> _perimeter = new();

        private Vector2 _cellTotal;

        public RaftQueries(Raft raft)
        {
            _raft = raft;

            _axes.Add(Axis.Horizontal, new RaftAxis(_raft, Axis.Horizontal));
            _axes.Add(Axis.Vertical, new RaftAxis(_raft, Axis.Vertical));

            foreach (KeyValuePair<Vector2Int, RaftTile> kvp in _raft.Tiles)
            {
                HandleTileChanged(kvp.Key, null, kvp.Value);
            }

            _raft.OnTileChanged += HandleTileChanged;
        }

        public void Dispose()
        {
            _raft.OnTileChanged -= HandleTileChanged;

            foreach (RaftAxis axis in _axes.Values)
            {
                axis.Dispose();
            }
        }

        private void HandleTileChanged(Vector2Int tileCell, RaftTile previous, RaftTile current)
        {
            if (previous != current)
            {
                if (current != null)
                {
                    _cellTotal += tileCell;
                }
                else
                {
                    _cellTotal -= tileCell;
                }
            }

            void processCell(Vector2Int offset)
            {
                Vector2Int cell = tileCell + offset;
                bool on = OnPerimeter(cell, out RaftPerimeterCell perimeterCell);

                if (!_perimeter.ContainsKey(cell))
                {
                    if (on)
                    {
                        _perimeter.Add(cell, perimeterCell);
                    }
                }
                else
                {
                    if (!on)
                    {
                        _perimeter.Remove(cell);
                    }
                }
            }

            for (int i = -1; i <= 1; i++)
            {
                processCell(new Vector2Int(i, 0));
            }

            for (int i = -1; i <= 1; i += 2)
            {
                processCell(new Vector2Int(0, i));
            }
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

        public Vector3 GetCenterPosition()
        {
            return CellToWorldPosition(_cellTotal / _raft.Tiles.Count);
        }

        // Retrieves a random tile that fulfills a predicate
        public bool TryGetRandomTile(Func<RaftTile, bool> predicate, out RaftTile tile)
        {
            tile = null;

            IEnumerable<RaftTile> tiles = _raft.Tiles.Values.Where(tile => predicate(tile));

            int count = tiles.Count();

            if (count == 0)
            {
                return false;
            }

            tile = tiles.ElementAt(Random.Range(0, count));

            return true;
        }

        public bool TryGetClosestTile(Vector3 position, out RaftTile tile)
        {
            tile = null;

            if (_raft.Tiles.Count == 0)
            {
                return false;
            }

            tile = _raft.Tiles.Values.OrderBy(tile => Vector3.Distance(tile.transform.position, position)).First();

            return true;
        }

        // Retrieves a random line
        public bool TryGetRandomLine(out RaftLine line)
        {
            line = null;

            IEnumerable<RaftLine> lines = Enumerable.Empty<RaftLine>();

            foreach (RaftAxis axis in _axes.Values)
            {
                lines = lines.Concat(axis.Lines.Values);
            }

            int count = lines.Count();

            if (count == 0)
            {
                return false;
            }

            line = lines.ElementAt(Random.Range(0, count));
            return true;
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

        // Finds the closest edge to a cell. Ties are resolved randomly
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

        private bool OnPerimeter(Vector2Int cell, out RaftPerimeterCell perimeterCell)
        {
            perimeterCell = null;

            if (!_raft.Tiles.ContainsKey(cell))
            {
                return false;
            }

            List<Direction> openDirections = new();

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (!_raft.Tiles.ContainsKey(cell + Utils.Math.DirectionToVector2Int(direction)))
                {
                    openDirections.Add(direction);
                }
            }

            if (openDirections.Count > 0)
            {
                perimeterCell = new RaftPerimeterCell(cell, openDirections);
            }
                
            return perimeterCell != null;
        }
    }
}