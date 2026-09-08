using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

namespace NoMoreFishAndChips.Environments
{
    // A RaftLine represents a span of cells along an index on an axis. It stores its ends into the values '_minEdge' and '_maxEdge'
    public class RaftLine
    {
        private RaftAxis _raftAxis;
        public RaftAxis RaftAxis => _raftAxis;

        private int _lineIndex;
        public int LineIndex => _lineIndex;

        private SortedDictionary<int, RaftLineNode> _nodes;
        public IReadOnlyDictionary<int, RaftLineNode> Nodes => _nodes;

        private RaftEdge _minEdge;
        private RaftEdge _maxEdge;

        public RaftEdge MinEdge => _minEdge;
        public RaftEdge MaxEdge => _maxEdge;

        public RaftLine(RaftAxis raftAxis, int lineIndex)
        {
            _raftAxis = raftAxis;
            _lineIndex = lineIndex;

            _nodes = new();
        }

        public void AddNode(int axisIndex)
        {
            _nodes.Add(axisIndex, new RaftLineNode(axisIndex, this));

            RefreshEdges();
        }

        public void RemoveNode(int axisIndex)
        {
            _nodes.Remove(axisIndex);

            RefreshEdges();
        }

        // Manual refresh when we know min and max are potentially dirty
        private void RefreshEdges()
        {
            if (_nodes.Count == 0)
            {
                _minEdge = null;
                _maxEdge = null;
                return;
            }

            RaftLineNode minNode = _nodes.First().Value;
            RaftLineNode maxNode = _nodes.Last().Value;

            _minEdge = new RaftEdge(minNode, _raftAxis.Type == Axis.Horizontal ? Direction.Left : Direction.Down);
            _maxEdge = new RaftEdge(maxNode, _raftAxis.Type == Axis.Horizontal ? Direction.Right : Direction.Up);
        }

        public RaftEdge GetEdge(int direction)
        {
            return direction < 0 ? _minEdge : _maxEdge;
        }

        public RaftEdge GetRandomEdge()
        {
            return Random.value < 0.5f ? _minEdge : _maxEdge;
        }

        public RaftLineNode GetNextNode(int axisIndex, int direction)
        {
            if (direction < 0)
            {
                return _nodes.Values.LastOrDefault(node => node.AxisIndex < axisIndex);
            }
            else
            {
                return _nodes.Values.FirstOrDefault(node => node.AxisIndex > axisIndex);
            }
        }

        public Vector2Int AxisIndexToCell(int axisIndex)
        {
            if (_raftAxis.Type == Axis.Horizontal)
            {
                return new Vector2Int(axisIndex, _lineIndex);
            }
            else
            {
                return new Vector2Int(_lineIndex, axisIndex);
            }
        }

        public Vector3 AxisIndexToWorldPosition(int axisIndex)
        {
            if (_raftAxis.Type == Axis.Horizontal)
            {
                return new Vector3(axisIndex, 0f, _lineIndex);
            }
            else
            {
                return new Vector3(_lineIndex, 0f, axisIndex);
            }
        }
    }
}