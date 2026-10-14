using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Utils;
using NoMoreFishAndChips.Entities;

namespace NoMoreFishAndChips.Environments
{
    public class RaftPerimeterNode
    {
        private Raft _raft;
        private RaftTile _tile;
        private List<Vector3> _openDirections = new();

        public RaftTile Tile => _tile;
        public IReadOnlyList<Vector3> OpenDirections => _openDirections;

        public RaftPerimeterNode(Raft raft, RaftTile tile)
        {
            _raft = raft;
            _tile = tile;

            RefreshOpenDirections();
        }

        public void RefreshOpenDirections()
        {
            _openDirections.Clear();

            void refresh(Vector2Int offset)
            {
                if (!_raft.Tiles.ContainsKey(_tile.Cell + offset))
                {
                    _openDirections.Add(new Vector3(offset.x, 0f, offset.y));
                }
            }

            for (int i = -1; i <= 1; i += 2)
            {
                refresh(new Vector2Int(i, 0));
                refresh(new Vector2Int(0, i));
            }
        }
    }
}