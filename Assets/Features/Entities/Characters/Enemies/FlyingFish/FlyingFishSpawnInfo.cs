using NoMoreFishAndChips.Environments;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class FlyingFishSpawnInfo : EnemySpawnInfo
    {
        private RaftLine _raftLine;
        public RaftLine RaftLine => _raftLine;

        public FlyingFishSpawnInfo(RaftLine line)
        {
            _raftLine = line;
        }
    }
}