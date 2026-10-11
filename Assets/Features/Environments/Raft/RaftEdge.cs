using UnityEngine;
using ShinyOwl.Common.Utils;

namespace NoMoreFishAndChips.Environments
{
    // A RaftEdge captures information about a tile that at the time of creation was considered on
    // the edge of the raft. This simply means that the tile had either the smallest or biggest value in a RaftLine
    public class RaftEdge
    {
        private RaftLineNode _node;
        private Vector2Int _direction2d;
        private Vector3 _direction3d;

        public RaftLineNode Node => _node;
        public Vector2Int Direction2d => _direction2d;
        public Vector3 Direction3d => _direction3d;

        public RaftEdge(RaftLineNode node, Vector2Int direction2d)
        {
            _node = node;
            _direction2d = direction2d;
            _direction3d = new Vector3(direction2d.x, 0f, direction2d.y);
        }
    }
}