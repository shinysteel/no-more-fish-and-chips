using UnityEngine;

namespace ShinyOwl.Common
{
    public class Path
    {
        [SerializeField] private Vector2Int[] _cells;

        public Vector2Int[] Cells => _cells;

        public Path(Vector2Int[] cells)
        {
            _cells = cells;
        }
    }
}