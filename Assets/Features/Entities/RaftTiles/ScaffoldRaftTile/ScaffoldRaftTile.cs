using UnityEngine;
using NoMoreFishAndChips.UI;

namespace NoMoreFishAndChips.Entities
{
    public class ScaffoldRaftTile : RaftTile
    {
        protected override bool CanPrompt()
        {
            return false;
        }

        protected override WorldUI CreatePromptUI()
        {
            return null;
        }

        protected override bool CanInteract()
        {
            return false;
        }

        protected override void Interact()
        { }
    }
}