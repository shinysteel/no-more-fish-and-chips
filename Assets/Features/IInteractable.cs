using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips
{
    public interface IInteractable
    {
        Transform transform { get; }
        IInteractableSettings IInteractableSettings { get; }
        bool CanPrompt();
        WorldUI CreatePromptUI();
        bool CanInteract();
        void Interact();
    }
}