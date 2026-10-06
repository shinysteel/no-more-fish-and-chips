using UnityEngine;

namespace ShinyOwl.Common
{
    public class Path
    {
        [SerializeField] private Vector2[] _positions;

        public Vector2[] Positions => _positions;

        public Path(Vector2[] positions)
        {
            _positions = positions;
        }
    }
}