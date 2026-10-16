using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerLogic : CharacterLogic
    {
        protected RaftPlayer _player;

        public RaftPlayerLogic(RaftPlayer player) : base(player)
        {
            _player = player;
        }
    }
}