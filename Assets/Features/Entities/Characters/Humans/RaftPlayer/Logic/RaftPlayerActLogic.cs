using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerActLogic : CharacterActLogic
    {
        private bool _inCutscene;

        public override bool CanAct => base.CanAct && !_inCutscene;

        public RaftPlayerActLogic(RaftPlayer player) : base(player)
        { }

        public void SetInCutscene(bool cutscene)
        {
            _inCutscene = cutscene;
        }
    }
}