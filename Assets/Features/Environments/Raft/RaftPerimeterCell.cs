using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Utils;

namespace NoMoreFishAndChips.Environments
{
    public class RaftPerimeterCell
    {
        private Vector2Int _cell;
        private List<Direction> _openDirections;

        public RaftPerimeterCell(Vector2Int cell, List<Direction> openDirections)
        {
            _cell = cell;
            _openDirections = openDirections;
        }
    }
}