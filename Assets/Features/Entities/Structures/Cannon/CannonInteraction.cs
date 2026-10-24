using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class CannonInteraction : GameplayInteraction
    {
        protected Cannon _cannon;

        public CannonInteraction(Cannon cannon, GameplayContext context) : base(context)
        {
            _cannon = cannon;
        }
    }
}