using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class SeagullSpawnInfo : EnemySpawnInfo
    {
        private RaftTile _tile;
        public RaftTile Tile => _tile;

        public SeagullSpawnInfo(RaftTile tile)
        {
            _tile = tile;
        }
    }
}