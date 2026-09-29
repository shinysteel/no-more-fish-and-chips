using UnityEngine;

namespace ShinyOwl.Common
{
    public interface IPathFindable
    {
        bool IsTraversable(Vector2Int cell);
    }
}