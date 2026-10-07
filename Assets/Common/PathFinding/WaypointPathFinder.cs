using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ShinyOwl.Common
{
    // Uses 8-direction A* to find paths before smoothing them by removing points where direct traversal is possible, creating waypoints
    public class WaypointPathFinder
    {
        private readonly IPathFindable _pathFindable;

        public WaypointPathFinder(IPathFindable findable)
        {
            _pathFindable = findable;
        }

        public bool TryFindPath(Vector2 startPosition, Vector2 endPosition, float radius, out Path path)
        {
            path = null;

            // Reject requests that start or end in non-traversable cells
            Vector2Int startCell = Vector2Int.RoundToInt(startPosition);
            Vector2Int endCell = Vector2Int.RoundToInt(endPosition);
            if (!_pathFindable.IsTraversable(startCell) || !_pathFindable.IsTraversable(endCell))
            {
                return false;
            }

            // Initial check for going from A to B
            if (CanTraverseLine(startPosition, endPosition, radius))
            {
                Vector2[] positions = new Vector2[] { startPosition, endPosition };
                path = new Path(positions);
                return true;
            }

            List<Vector2Int> pathCells = ListPool<Vector2Int>.Get();
            List<Vector2> waypoints = ListPool<Vector2>.Get();

            try
            {
                // Standard A*
                if (!TryFindShortestPath(pathCells, startCell, endCell, radius))
                {
                    return false;
                }

                waypoints.Add(startPosition);

                for (int i = 1; i < pathCells.Count - 1; i++)
                {
                    waypoints.Add(pathCells[i]);
                }

                waypoints.Add(endPosition);

                // Smooth where possible
                SmoothPath(waypoints, radius);

                path = new Path(waypoints.ToArray());

                return true;
            }
            finally
            {
                ListPool<Vector2Int>.Release(pathCells);
                ListPool<Vector2>.Release(waypoints);
            }
        }

        // Runs several distance checks to determine if the line from A to B is traversable with respect to a radius
        private bool CanTraverseLine(Vector2 startPosition, Vector2 endPosition, float radius)
        {
            // Checks if a point is in a rectangle
            bool inRectangle(Vector2 point, float minX, float maxX, float minY, float maxY)
            {
                return point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY;
            }

            // Checks if two lines are intersecting
            bool isIntersecting(Vector2 startA, Vector2 endA, Vector2 startB, Vector2 endB)
            {
                Vector2 vectorA = endA - startA;
                Vector2 vectorB = endB - startB;

                float cross = vectorA.x * vectorB.y - vectorA.y * vectorB.x;

                if (Mathf.Abs(cross) <= Mathf.Epsilon)
                {
                    return false;
                }

                Vector2 startVector = startB - startA;

                float progressA = (startVector.x * vectorB.y - startVector.y * vectorB.x) / cross;
                float progressB = (startVector.x * vectorA.y - startVector.y * vectorA.x) / cross;

                return progressA >= 0f && progressA <= 1f && progressB >= 0f && progressB <= 1f;
            }

            // Finds the closest distance between a point and any point on a line (segment)
            float getPointSegmentDistanceSquared(Vector2 point, Vector2 start, Vector2 end)
            {
                Vector2 lineVector = end - start;
                float magnitudeSquared = lineVector.sqrMagnitude;

                if (magnitudeSquared <= Mathf.Epsilon)
                {
                    return (point - start).sqrMagnitude;
                }

                float lineProgress = Mathf.Clamp01(Vector2.Dot(point - start, lineVector) / magnitudeSquared);
                Vector2 closestPoint = start + lineVector * lineProgress;

                return (point - closestPoint).sqrMagnitude;
            }

            // Finds the closest distance between any point on a line (segment) and a cell (rectangle) 
            float getSegmentRectangleDistanceSquared(Vector2 start, Vector2 end, Vector2Int cell)
            {
                float minX = cell.x - 0.5f;
                float maxX = cell.x + 0.5f;
                float minY = cell.y - 0.5f;
                float maxY = cell.y + 0.5f;

                Vector2 bottomLeft = new Vector2(minX, minY);
                Vector2 bottomRight = new Vector2(maxX, minY);
                Vector2 topRight = new Vector2(maxX, maxY);
                Vector2 topLeft = new Vector2(minX, maxY);

                if (inRectangle(start, minX, maxX, minY, maxY) || inRectangle(end, minX, maxX, minY, maxY))
                {
                    return 0f;
                }

                if (isIntersecting(start, end, bottomLeft, bottomRight)
                    || isIntersecting(start, end, bottomRight, topRight)
                    || isIntersecting(start, end, topRight, topLeft)
                    || isIntersecting(start, end, topLeft, bottomLeft))
                {
                    return 0f;
                }

                float distanceSquared = float.PositiveInfinity;

                distanceSquared = Mathf.Min(distanceSquared, getPointSegmentDistanceSquared(bottomLeft, start, end));
                distanceSquared = Mathf.Min(distanceSquared, getPointSegmentDistanceSquared(bottomRight, start, end));
                distanceSquared = Mathf.Min(distanceSquared, getPointSegmentDistanceSquared(topRight, start, end));
                distanceSquared = Mathf.Min(distanceSquared, getPointSegmentDistanceSquared(topLeft, start, end));
                distanceSquared = Mathf.Min(distanceSquared, getPointSegmentDistanceSquared(start, bottomLeft, bottomRight));
                distanceSquared = Mathf.Min(distanceSquared, getPointSegmentDistanceSquared(start, topRight, topLeft));
                distanceSquared = Mathf.Min(distanceSquared, getPointSegmentDistanceSquared(end, bottomLeft, bottomRight));
                distanceSquared = Mathf.Min(distanceSquared, getPointSegmentDistanceSquared(end, topRight, topLeft));

                return distanceSquared;
            }

            // Create a rectangle bounds to contain all cells that are relevant to travelling from A to B. This is inherently conservative on diagonals
            float minX = Mathf.Min(startPosition.x, endPosition.x) - radius - 0.5f;
            float maxX = Mathf.Max(startPosition.x, endPosition.x) + radius + 0.5f;
            float minY = Mathf.Min(startPosition.y, endPosition.y) - radius - 0.5f;
            float maxY = Mathf.Max(startPosition.y, endPosition.y) + radius + 0.5f;

            int startX = Mathf.FloorToInt(minX);
            int endX = Mathf.FloorToInt(maxX);
            int startY = Mathf.FloorToInt(minY);
            int endY = Mathf.FloorToInt(maxY);

            float radiusSquared = radius * radius;

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);

                    if (_pathFindable.IsTraversable(cell))
                    {
                        continue;
                    }

                    if (getSegmentRectangleDistanceSquared(startPosition, endPosition, cell) <= radiusSquared)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // 8-direction A* algorithim
        private bool TryFindShortestPath(List<Vector2Int> pathCells, Vector2Int startCell, Vector2Int endCell, float radius)
        {
            Dictionary<Vector2Int, float> cellCosts = DictionaryPool<Vector2Int, float>.Get();
            Dictionary<Vector2Int, Vector2Int> parentCells = DictionaryPool<Vector2Int, Vector2Int>.Get();
            HashSet<Vector2Int> closedCells = HashSetPool<Vector2Int>.Get();
            List<Vector2Int> openCells = ListPool<Vector2Int>.Get();

            float getEstimatedCost(Vector2Int cell, Vector2Int endCell)
            {
                float cost = cellCosts[cell];

                int distanceX = Mathf.Abs(endCell.x - cell.x);
                int distanceY = Mathf.Abs(endCell.y - cell.y);

                int diagonalDistance = Mathf.Min(distanceX, distanceY);
                int straightDistance = Mathf.Max(distanceX, distanceY) - diagonalDistance;

                float heuristic = diagonalDistance * Utils.Utils.Math.SquareRootOfTwo + straightDistance;

                return cost + heuristic;
            }

            try
            {
                cellCosts[startCell] = 0f;
                openCells.Add(startCell);

                while (openCells.Count > 0)
                {
                    int bestIndex = 0;
                    float bestCost = getEstimatedCost(openCells[0], endCell);

                    for (int i = 1; i < openCells.Count; i++)
                    {
                        float cost = getEstimatedCost(openCells[i], endCell);

                        if (cost < bestCost)
                        {
                            bestCost = cost;
                            bestIndex = i;
                        }
                    }

                    Vector2Int currentCell = openCells[bestIndex];
                    openCells.RemoveAt(bestIndex);

                    if (currentCell == endCell)
                    {
                        Vector2Int cell = endCell;

                        while (true)
                        {
                            pathCells.Add(cell);

                            if (cell == startCell)
                            {
                                break;
                            }

                            cell = parentCells[cell];
                        }

                        pathCells.Reverse();
                        return true;
                    }

                    closedCells.Add(currentCell);

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            Vector2Int direction = new Vector2Int(i, j);
                            Vector2Int neighbour = currentCell + direction;

                            if (closedCells.Contains(neighbour))
                            {
                                continue;
                            }

                            if (!_pathFindable.IsTraversable(neighbour))
                            {
                                continue;
                            }

                            if (direction.x != 0 && direction.y != 0)
                            {
                                Vector2Int horizontal = currentCell + new Vector2Int(direction.x, 0);
                                Vector2Int vertical = currentCell + new Vector2Int(0, direction.y);

                                if (!_pathFindable.IsTraversable(horizontal) || !_pathFindable.IsTraversable(vertical))
                                {
                                    continue;
                                }
                            }

                            float movementCost = direction.x != 0 && direction.y != 0  ? Utils.Utils.Math.SquareRootOfTwo : 1f;
                            float newCost = cellCosts[currentCell] + movementCost;

                            if (!cellCosts.TryGetValue(neighbour, out float oldCost))
                            {
                                cellCosts[neighbour] = newCost;
                                parentCells[neighbour] = currentCell;
                                openCells.Add(neighbour);
                            }
                            else if (newCost < oldCost)
                            {
                                cellCosts[neighbour] = newCost;
                                parentCells[neighbour] = currentCell;

                                if (!openCells.Contains(neighbour))
                                {
                                    openCells.Add(neighbour);
                                }
                            }
                        }
                    }
                }

                return false;
            }
            finally
            {
                DictionaryPool<Vector2Int, float>.Release(cellCosts);
                DictionaryPool<Vector2Int, Vector2Int>.Release(parentCells);
                HashSetPool<Vector2Int>.Release(closedCells);
                ListPool<Vector2Int>.Release(openCells);
            }
        }

        // Iterates a path and removes points when upcoming points can be directly traversed to
        private void SmoothPath(List<Vector2> waypoints, float radius)
        {
            if (waypoints.Count <= 2)
            {
                return;
            }

            int currentIndex = 0;

            while (currentIndex < waypoints.Count - 2)
            {
                bool removed = false;

                for (int nextIndex = waypoints.Count - 1; nextIndex > currentIndex + 1; nextIndex--)
                {
                    if (!CanTraverseLine(waypoints[currentIndex], waypoints[nextIndex], radius))
                    {
                        continue;
                    }

                    waypoints.RemoveRange(currentIndex + 1, nextIndex - currentIndex - 1);

                    removed = true;
                    break;
                }

                if (!removed)
                {
                    currentIndex++;
                }
            }
        }
    }
}