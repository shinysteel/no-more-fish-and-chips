using NoMoreFishAndChips.Entities;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    // A context container for both a Tile and it's AxisIndex within a RaftLine
    public class RaftLineNode : IComparable<RaftLineNode>
    {
        private RaftLine _line;
        private int _axisIndex;
        private RaftTile _tile;

        public int AxisIndex => _axisIndex;
        public RaftTile Tile => _tile;

        public RaftLineNode(RaftLine line, int axisIndex, RaftTile tile)
        {
            _line = line;
            _axisIndex = axisIndex;
            _tile = tile;
        }

        public int CompareTo(RaftLineNode other)
        {
            return _axisIndex.CompareTo(other._axisIndex);
        }
    }
}