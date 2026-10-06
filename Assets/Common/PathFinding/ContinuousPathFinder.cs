using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace ShinyOwl.Common
{
    public class ContinuousPathFinder
    {
        public static List<Vector2> TestVertices;
        
        private IPathFindable _pathFindable;

        private class Edge : IEquatable<Edge>
        {
            public Vector2 Start { get; private set; }
            public Vector2 End { get; private set; }
            public Vector2Int Normal { get; private set; }

            public Edge(Vector2 start, Vector2 end, Vector2Int normal)
            {
                Start = start;
                End = end;
                Normal = normal;
            }

            public bool Equals(Edge other)
            {
                return (Start == other.Start && End == other.End) || (Start == other.End && End == other.Start);
            }

            public override int GetHashCode()
            {
                return Start.GetHashCode() ^ End.GetHashCode();
            }
        }

        public ContinuousPathFinder(IPathFindable findable)
        {
            _pathFindable = findable;
        }

        public void OnDrawGizmos()
        {
            if (TestVertices == null)
            {
                return;
            }

            foreach (Vector2 vertex in TestVertices)
            {
                Vector3 position = vertex * 0.5f - Vector2.one * 0.25f;
                position = new Vector3(position.x, 0.25f, position.y);

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(position, 0.1f);
            }
        }

        public bool TryFindPath(Vector2 startPosition, Vector2 endPosition, float radius, out Path path)
        {
            path = null;
            TestVertices = null;

            Vector2Int startCell = Vector2Int.RoundToInt(startPosition);
            Vector2Int endCell = Vector2Int.RoundToInt(endPosition);

            Log.Info($"path finding from grid position {startPosition} to {endPosition} which converts to grid cells {startCell} to {endCell}");

            if (!_pathFindable.IsTraversable(startCell) || !_pathFindable.IsTraversable(endCell))
            {
                Log.Info($"invalid request to find a path, since starting or ending on a non-traversable tile");
                return false;
            }

            // If the direct line from A to B is traversable, the path is solved
            if (CanTraverseLine(startPosition, endPosition, radius))
            {
                Vector2[] positions = new Vector2[] { startPosition, endPosition };
                path = new Path(positions);
                Log.Info($"a direct line from A to B was indentified, simple path");
                return true;
            }

            List<Vector2Int> traversableCells = ListPool<Vector2Int>.Get();
            List<Vector2> vertices = ListPool<Vector2>.Get();
            List<List<int>> neighbours = ListPool<List<int>>.Get();
            List<int> indices = ListPool<int>.Get();

            try
            {
                // Find all relevant cells we can traverse to from startPosition
                FloodFillTraversableCells(traversableCells, startCell, endCell);

                if (!traversableCells.Contains(endCell))
                {
                    Log.Info("after flood filling, tarversable cells does not contain end cell");
                    return false;
                }

                // Determine navigational boundaries by expanding cell edges
                DetermineVertices(vertices, traversableCells, radius);

                vertices.Insert(0, startPosition);
                vertices.Add(endPosition);

                Log.Info($"vertices count: {vertices.Count}");

                // Create the visibility graph using the offset vertices
                CreateVisibilityGraph(neighbours, vertices, radius);

                if (!TryFindShortestPath(indices, vertices, neighbours, vertices.Count - 1))
                {
                    Log.Info($"failed to find any path");
                    return false;
                }

                Log.Info($"found valid path with positions:");

                Vector2[] positions = new Vector2[indices.Count];

                for (int i = 0; i < indices.Count; i++)
                {
                    positions[i] = vertices[indices[i]];
                    Log.Info($"{i}: {positions[i]}");
                }

                path = new Path(positions);
                return true;
            }
            finally
            {
                TestVertices = vertices.ToList();

                foreach (List<int> list in neighbours)
                {
                    ListPool<int>.Release(list);
                }

                ListPool<Vector2Int>.Release(traversableCells);
                ListPool<Vector2>.Release(vertices);
                ListPool<List<int>>.Release(neighbours);
                ListPool<int>.Release(indices);
            }
        }

        private bool CanTraverseLine(Vector3 startPosition, Vector3 endPosition, float radius)
        {
            bool canTraverse(Vector2 position)
            {
                int minX = Mathf.FloorToInt(position.x - radius - 0.5f);
                int maxX = Mathf.FloorToInt(position.x + radius + 0.5f);

                int minY = Mathf.FloorToInt(position.y - radius - 0.5f);
                int maxY = Mathf.FloorToInt(position.y + radius + 0.5f);

                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        Vector2Int cell = new Vector2Int(x, y);

                        if (!hasOverlap(cell, position, radius))
                        {
                            continue;
                        }

                        if (!_pathFindable.IsTraversable(cell))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            // Circular overlap
            bool hasOverlap(Vector2Int cell, Vector2 position, float radius)
            {
                float minX = cell.x - 0.5f;
                float maxX = cell.x + 0.5f;

                float minY = cell.y - 0.5f;
                float maxY = cell.y + 0.5f;

                float closestX = Mathf.Clamp(position.x, minX, maxX);
                float closestY = Mathf.Clamp(position.y, minY, maxY);

                float deltaX = position.x - closestX;
                float deltaY = position.y - closestY;

                return Mathf.Pow(deltaX, 2f) + Mathf.Pow(deltaY, 2f) < Mathf.Pow(radius, 2f);
            }

            float step = 0.5f;
            float distance = (endPosition - startPosition).magnitude;

            int steps = Mathf.CeilToInt(distance / step);
            steps = Mathf.Max(steps, 1);

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2 position = Vector2.Lerp(startPosition, endPosition, t);

                if (!canTraverse(position))
                {
                    return false;
                }
            }

            return true;
        }

        private void FloodFillTraversableCells(List<Vector2Int> cells, Vector2Int startCell, Vector2Int endCell)
        {
            List<Vector2Int> openCells = ListPool<Vector2Int>.Get();
            HashSet<Vector2Int> closedCells = HashSetPool<Vector2Int>.Get();

            try
            {
                openCells.Add(startCell);
                closedCells.Add(startCell);

                while (openCells.Count > 0)
                {
                    Vector2Int cell = openCells[0];
                    openCells.RemoveAt(0);

                    if (!_pathFindable.IsTraversable(cell))
                    {
                        continue;
                    }

                    cells.Add(cell);

                    if (cell == endCell)
                    {
                        return;
                    }

                    void add(Vector2Int cell)
                    {
                        if (!closedCells.Contains(cell))
                        {
                            closedCells.Add(cell);
                            openCells.Add(cell);
                        }
                    }

                    for (int i = -1; i <= 1; i += 2)
                    {
                        add(cell + new Vector2Int(i, 0));
                        add(cell + new Vector2Int(0, i));
                    }
                }
            }
            finally
            {
                ListPool<Vector2Int>.Release(openCells);
                HashSetPool<Vector2Int>.Release(closedCells);
            }
        }

        private void DetermineVertices(List<Vector2> vertices, List<Vector2Int> traversableCells, float radius)
        {
            HashSet<Edge> edges = HashSetPool<Edge>.Get();
            Dictionary<Vector2, Vector2Int> cornerNormals = DictionaryPool<Vector2, Vector2Int>.Get();

            void addCorner(Vector2 corner, Vector2Int normal)
            {
                cornerNormals.TryGetValue(corner, out Vector2Int normals);
                cornerNormals[corner] = normals + normal;
            }

            foreach (Vector2Int cell in traversableCells)
            {
                void addEdge(Vector2Int direction)
                {
                    Vector2Int neighbour = cell + direction;

                    if (_pathFindable.IsTraversable(neighbour))
                    {
                        return;
                    }

                    Vector2 start = cell + (Vector2)direction * 0.5f;
                    Vector2 end = start;

                    Vector2Int mask = new Vector2Int(direction.y, direction.x);

                    start -= (Vector2)mask * 0.5f;
                    end += (Vector2)mask * 0.5f;

                    Edge edge = new Edge(start, end, direction);

                    if (edges.Add(edge))
                    {
                        addCorner(edge.Start, edge.Normal);
                        addCorner(edge.End, edge.Normal);
                    }
                }

                for (int i = -1; i <= 1; i += 2)
                {
                    addEdge(new Vector2Int(i, 0));
                    addEdge(new Vector2Int(0, i));
                }
            }

            foreach (Edge edge in edges)
            {
                vertices.Add(edge.Start - (Vector2)edge.Normal * radius);
                vertices.Add(edge.End - (Vector2)edge.Normal * radius);
            }

            if (radius > 0f)
            {
                foreach (KeyValuePair<Vector2, Vector2Int> kvp in cornerNormals)
                {
                    if (Mathf.Abs(kvp.Value.x) != 1 || Mathf.Abs(kvp.Value.y) != 1)
                    {
                        continue;
                    }

                    Vector2 diagonal = (Vector2)kvp.Value * 0.5f;

                    if (_pathFindable.IsTraversable(Vector2Int.RoundToInt(kvp.Key + diagonal)))
                    {
                        continue;
                    }

                    if (!_pathFindable.IsTraversable(Vector2Int.RoundToInt(kvp.Key - diagonal)))
                    {
                        continue;
                    }

                    Vector2 startDirection = new Vector2(-kvp.Value.x, 0f);
                    Vector2 endDirection = new Vector2(0f, -kvp.Value.y);

                    float startAngle = Mathf.Atan2(startDirection.y, startDirection.x);
                    float endAngle = Mathf.Atan2(endDirection.y, endDirection.x);

                    int segmentCount = 4;
                    float deltaAngle = Mathf.DeltaAngle(startAngle * Mathf.Rad2Deg, endAngle * Mathf.Rad2Deg);
                    float stepAngle = deltaAngle * Mathf.Deg2Rad / segmentCount;

                    float arcRadius = radius / Mathf.Cos(Mathf.Abs(stepAngle) * 0.5f);

                    for (int i = 0; i <= segmentCount; i++)
                    {
                        float angle = startAngle + stepAngle * i;
                        vertices.Add(kvp.Key + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * arcRadius);
                    }
                }
            }

            HashSetPool<Edge>.Release(edges);
            DictionaryPool<Vector2, Vector2Int>.Release(cornerNormals);
        }

        private void CreateVisibilityGraph(List<List<int>> neighbours, List<Vector2> vertices, float radius)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                List<int> list = ListPool<int>.Get();
                neighbours.Add(list);
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = i + 1; j < vertices.Count; j++)
                {
                    if (CanTraverseLine(vertices[i], vertices[j], radius))
                    {
                        neighbours[i].Add(j);
                        neighbours[j].Add(i);
                    }
                }
            }
        }

        private bool TryFindShortestPath(List<int> indices, List<Vector2> vertices, List<List<int>> neighbours, int endIndex)
        {
            List<float> vertexCosts = ListPool<float>.Get();
            List<int> vertexParents = ListPool<int>.Get();
            List<bool> closedVertices = ListPool<bool>.Get();

            try
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    vertexCosts.Add(float.PositiveInfinity);
                    vertexParents.Add(-1);
                    closedVertices.Add(false);
                }

                vertexCosts[0] = 0f;

                for (int i = 0; i < vertices.Count; i++)
                {
                    int current = -1;
                    float bestCost = float.PositiveInfinity;

                    for (int j = 0; j < vertices.Count; j++)
                    {
                        if (closedVertices[j])
                        {
                            continue;
                        }

                        if (vertexCosts[j] < bestCost)
                        {
                            bestCost = vertexCosts[j];
                            current = j;
                        }
                    }

                    if (current == -1)
                    {
                        return false;
                    }

                    if (current == endIndex)
                    {
                        // Construct the path
                        int index = endIndex;

                        while (index != -1)
                        {
                            indices.Add(index);
                            index = vertexParents[index];
                        }

                        indices.Reverse();

                        return true;
                    }

                    closedVertices[current] = true;

                    foreach (int neighbour in neighbours[current])
                    {
                        if (closedVertices[neighbour])
                        {
                            continue;
                        }

                        float cost = vertexCosts[current] + Vector2.Distance(vertices[current], vertices[neighbour]);

                        if (cost < vertexCosts[neighbour])
                        {
                            vertexCosts[neighbour] = cost;
                            vertexParents[neighbour] = current;
                        }
                    }
                }

                return false;
            }
            finally
            {
                ListPool<float>.Release(vertexCosts);
                ListPool<int>.Release(vertexParents);
                ListPool<bool>.Release(closedVertices);
            }
        }
    }
}