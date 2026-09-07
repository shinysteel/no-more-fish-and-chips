using NoMoreFishAndChips.Environments;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class FlyingFishSpawnInfo : EnemySpawnInfo
    {
        private RaftLine _line;
        public RaftLine Line => _line;

        public FlyingFishSpawnInfo(RaftLine line)
        {
            _line = line;
        }
    }
}