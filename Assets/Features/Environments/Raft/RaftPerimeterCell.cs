using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Utils;

namespace NoMoreFishAndChips.Environments
{
    public class RaftPerimeterCell
    {
        private Vector2Int _cell;
        private List<Vector3> _openDirections;

        public RaftPerimeterCell(Vector2Int cell, List<Vector3> openDirections)
        {
            _cell = cell;
            _openDirections = openDirections;
        }
    }
}