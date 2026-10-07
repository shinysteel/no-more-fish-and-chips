using UnityEngine;

namespace ShinyOwl.Common
{
    public class PathNavigator
    {
        private IPathFindable _pathFindable;
        private float _radius;

        private WaypointPathFinder _pathFinder;

        private Path _path;
        private int _index;

        public PathNavigator(IPathFindable findable, float radius)
        {
            _pathFindable = findable;
            _radius = radius;

            _pathFinder = new WaypointPathFinder(_pathFindable);
        }

        public bool HasPath()
        {
            return _path != null;
        }

        public void ClearPath()
        {
            _path = null;
            _index = 0;
        }

        public bool TrySetPath(Vector2 startPosition, Vector2 endPosition)
        {
            if (!_pathFinder.TryFindPath(startPosition, endPosition, _radius, out Path path))
            {
                return false;
            }

            _path = path;
            _index = 0;

            return true;
        }

        public void Tick(Vector2 position)
        {   
            if (AtDestination())
            {
                return;
            }

            if (Vector2.Distance(position, _path.Positions[_index + 1]) < _radius)
            {
                _index++;
            }
        }

        public bool AtDestination()
        {
            return _index == _path.Positions.Length - 1;
        }

        public Vector2 GetNextPosition()
        {
            int index = _index + 1;
            index = Mathf.Min(index, _path.Positions.Length - 1);

            return _path.Positions[index];
        }
    }
}