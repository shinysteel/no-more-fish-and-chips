using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerContextLogic : RaftPlayerLogic
    {
        private UIManager _uiManager;

        public RaftPlayerContextLogic(RaftPlayer player) : base(player)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
        }
    }
}