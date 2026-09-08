using System;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    // A context container for both a Tile and it's AxisIndex within a RaftLine
    public class RaftLineNode : IComparable<RaftLineNode>
    {
        private int _axisIndex;
        public int AxisIndex => _axisIndex;

        private RaftLine _line;

        private Vector2Int _cell;
        public Vector2Int Cell => _cell;

        public RaftLineNode(int axisIndex, RaftLine line)
        {
            _axisIndex = axisIndex;
            _line = line;
            _cell = _line.AxisIndexToCell(_axisIndex);
        }

        public int CompareTo(RaftLineNode other)
        {
            return _axisIndex.CompareTo(other._axisIndex);
        }
    }
}