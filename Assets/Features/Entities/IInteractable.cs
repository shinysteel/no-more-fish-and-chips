using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public interface IInteractable
    {
        Transform transform { get; }
        IInteractableSettings Settings { get; }
        bool CanPrompt();
        WorldUI CreatePromptUI();
        bool CanInteract();
        void Interact();
    }
}