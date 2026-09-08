using UnityEngine;
using ShinyOwl.Common.Utils;

namespace NoMoreFishAndChips.Environments
{
    // A RaftEdge captures information about a tile that at the time of creation was considered on
    // the edge of the raft. This simply means that the tile had either the smallest or biggest value in a RaftLine
    public class RaftEdge
    {
        private RaftLineNode _node;
        private Direction _direction;

        public RaftLineNode Node => _node;
        public Direction Direction => _direction;

        public RaftEdge(RaftLineNode node, Direction direction)
        {
            _node = node;
            _direction = direction;
        }
    }
}