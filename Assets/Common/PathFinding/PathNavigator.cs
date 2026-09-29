using UnityEngine;

namespace ShinyOwl.Common
{
    public class PathNavigator
    {
        private IPathFindable _pathFindable;
        private GreedyPathFinder _pathFinder;

        private Path _path;
        private int _index;

        public PathNavigator(IPathFindable findable)
        {
            _pathFindable = findable;
            _pathFinder = new GreedyPathFinder(_pathFindable);
        }

        public bool HasPath()
        {
            return _path != null;
        }

        public bool TrySetPath(Vector2Int startCell, Vector2Int endCell)
        {
            if (!_pathFinder.TryFindPath(startCell, endCell, out Path path))
            {
                return false;
            }

            _path = path;
            _index = 0;

            return true;
        }

        public void Tick(Vector2Int cell)
        {
            if (AtDestination())
            {
                return;
            }

            if (_path.Cells[_index + 1] == cell)
            {
                _index++;
            }
        }

        public bool AtDestination()
        {
            return _index == _path.Cells.Length - 1;
        }

        public bool AtIndex(Vector2Int cell)
        {
            return cell == _path.Cells[_index];
        }

        public Vector2Int GetDirection()
        {
            if (AtDestination())
            {
                return Vector2Int.zero;
            }

            return (_path.Cells[_index + 1] - _path.Cells[_index]);
        }
    }
}