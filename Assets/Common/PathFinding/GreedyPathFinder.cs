using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

namespace ShinyOwl.Common
{
    public class GreedyPathFinder
    {
        private IPathFindable _pathFindable;

        public GreedyPathFinder(IPathFindable findable)
        {
            _pathFindable = findable;
        }

        public bool TryFindPath(Vector2Int startCell, Vector2Int endCell, out Path path)
        {
            path = null;

            if (!_pathFindable.IsTraversable(startCell) || !_pathFindable.IsTraversable(endCell))
            {
                return false;
            }

            List<Vector2Int> openCells = ListPool<Vector2Int>.Get();
            HashSet<Vector2Int> closedCells = HashSetPool<Vector2Int>.Get();
            List<Vector2Int> neighbourCells = ListPool<Vector2Int>.Get();
            Dictionary<Vector2Int, Vector2Int> parentCells = DictionaryPool<Vector2Int, Vector2Int>.Get();

            openCells.Add(startCell);

            while (openCells.Count > 0)
            {
                Vector2Int closestCell = Vector2Int.zero;
                int closestDistance = int.MaxValue;

                for (int i = 0; i < openCells.Count; i++)
                {
                    int distance = Utils.Utils.Math.ManhattanDistance(openCells[i], endCell);

                    if (distance < closestDistance)
                    {
                        closestCell = openCells[i];
                        closestDistance = distance;
                    }
                }

                openCells.Remove(closestCell);
                closedCells.Add(closestCell);

                if (closestCell == endCell)
                {
                    List<Vector2Int> cells = ListPool<Vector2Int>.Get();

                    Vector2Int cell = endCell;

                    while (cell != startCell)
                    {
                        cells.Add(cell);
                        cell = parentCells[cell];
                    }

                    cells.Add(startCell);
                    cells.Reverse();

                    path = new Path(cells.ToArray());

                    ListPool<Vector2Int>.Release(cells);

                    break;
                }

                neighbourCells.Clear();

                void addNeighbour(Vector2Int offset)
                {
                    neighbourCells.Add(closestCell + offset);
                }

                for (int i = -1; i <= 1; i += 2)
                {
                    addNeighbour(new Vector2Int(i, 0));
                    addNeighbour(new Vector2Int(0, i));
                }

                foreach (Vector2Int cell in neighbourCells)
                {
                    if (closedCells.Contains(cell))
                    {
                        continue;
                    }

                    if (!_pathFindable.IsTraversable(cell))
                    {
                        continue;
                    }

                    if (!openCells.Contains(cell))
                    {
                        parentCells[cell] = closestCell;
                        openCells.Add(cell);
                    }
                }
            }

            ListPool<Vector2Int>.Release(openCells);
            HashSetPool<Vector2Int>.Release(closedCells);
            ListPool<Vector2Int>.Release(neighbourCells);
            DictionaryPool<Vector2Int, Vector2Int>.Release(parentCells);

            return path != null;
        }
    }
}