using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Utils;
using NoMoreFishAndChips.Entities;

namespace NoMoreFishAndChips.Environments
{
    public class RaftPerimeterNode
    {
        private RaftTile _tile;
        private List<Vector3> _openDirections;

        public RaftTile Tile => _tile;
        public List<Vector3> OpenDirections => _openDirections;

        public RaftPerimeterNode(RaftTile tile, List<Vector3> openDirections)
        {
            _tile = tile;
            _openDirections = openDirections;
        }
    }
}