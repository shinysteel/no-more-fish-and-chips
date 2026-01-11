using ShinyOwl.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FishFlingers.Entities;
using FishFlingers.Networking;

namespace FishFlingers.Environments
{
    public enum RaftSide
    {
        Forward ,
        Back    ,
        Right   ,
        Left    ,
    }
    
    /// <summary>
    /// A tile on the perimeter of the raft
    /// </summary>
    public struct RaftEdge
    {
        public RaftTile Tile { get; private set; }
        public Vector2Int Direction2D { get; private set; }
        public Vector3 Direction3D { get; private set; }

        public RaftEdge(RaftTile tile, Vector2Int direction2D, Vector3 direction3D)
        {
            Tile = tile;
            Direction2D = direction2D;
            Direction3D = direction3D;
        }
    }

    public partial class Raft : NetBehaviour
    {
        // Uses Vector2 to allow for floating-point cells
        public Vector3 CellToWorldPosition(Vector2 cell)
        {
            return new Vector3(cell.x, 0f, cell.y);
        }

        /// <summary>
        /// Finds the closest edge to a cell. Ties are resolved randomly
        /// </summary>
        public bool TryGetClosestEdge(Vector2Int cell, out RaftEdge edge)
        {
            edge = default;

            if (!_columnToRowsMap.TryGetValue(cell.x, out SortedSet<int> column) || !_rowToColumnsMap.TryGetValue(cell.y, out SortedSet<int> row))
            {
                return false;
            }

            // Determine the edges relative to the cell
            RaftEdge forwardEdge = new RaftEdge(_tiles[new Vector2Int(cell.x, column.Max)], Vector2Int.up, Vector3.forward);
            RaftEdge backEdge = new RaftEdge(_tiles[new Vector2Int(cell.x, column.Min)], Vector2Int.down, Vector3.back);
            RaftEdge rightEdge = new RaftEdge(_tiles[new Vector2Int(row.Max, cell.y)], Vector2Int.right, Vector3.right);
            RaftEdge leftEdge = new RaftEdge(_tiles[new Vector2Int(row.Min, cell.y)], Vector2Int.left, Vector3.left);

            // Pair the edges with their distance from the cell
            (RaftEdge edge, int dist)[] edges = new (RaftEdge, int)[]
            {
                (forwardEdge, Mathf.Abs(cell.y - forwardEdge.Tile.Cell.y)),
                (backEdge, Mathf.Abs(cell.y - backEdge.Tile.Cell.y)),
                (rightEdge, Mathf.Abs(cell.x - rightEdge.Tile.Cell.x)),
                (leftEdge, Mathf.Abs(cell.x - leftEdge.Tile.Cell.x))
            };

            int minDist = edges.Min(edge => edge.dist);

            edge = edges
                .Where(edge => edge.dist == minDist)
                .OrderBy(_ => Random.value)
                .First()
                .edge;

            return true;
        }

        /// <summary>
        /// Retrieves a random tile
        /// </summary>
        public bool TryGetRandomTile(out RaftTile tile)
        {
            tile = null;

            if (_tiles.Values.Count == 0)
            {
                return false;
            }
            
            tile = _tiles.Values.ElementAt(Random.Range(0, _tiles.Count));
            return true;
        }


        /// <summary>
        /// Gets the furthest tile on the raft's side. Ties are resolved randomly
        /// </summary>
        public bool TryGetBoundaryTile(RaftSide side, out RaftTile tile)
        {
            tile = null;

            if (_tiles.Values.Count == 0)
            {
                return false;
            }

            int x;
            int y;

            switch (side)
            {
                case RaftSide.Forward:
                    x = _rowToColumnsMap.Keys.Max();
                    y = _rowToColumnsMap[x].OrderBy(_ => Random.value).First();
                    break;

                case RaftSide.Back:
                    x = _rowToColumnsMap.Keys.Min();
                    y = _rowToColumnsMap[x].OrderBy(_ => Random.value).First();
                    break;

                case RaftSide.Right:
                    y = _columnToRowsMap.Keys.Max();
                    x = _columnToRowsMap[y].OrderBy(_ => Random.value).First();
                    break;

                case RaftSide.Left:
                    y = _columnToRowsMap.Keys.Min();
                    x = _columnToRowsMap[y].OrderBy(_ => Random.value).First();
                    break;

                default:
                    x = 0;
                    y = 0;
                    break;
            }

            tile = _tiles[new Vector2Int(x, y)];
            return true;
        }
    }
}