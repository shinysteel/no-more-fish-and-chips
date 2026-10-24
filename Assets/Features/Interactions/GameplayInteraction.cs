using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Interactions
{
    public abstract class GameplayInteraction : Interaction
    {
        protected GameplayContext _context;

        public GameplayInteraction(GameplayContext context) : base()
        {
            _context = context;
        }

        public override bool CanPrompt()
        {
            return _context != null;
        }
    }
}