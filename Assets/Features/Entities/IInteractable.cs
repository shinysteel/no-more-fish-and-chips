using FishFlingers.UI;
using UnityEngine;

namespace FishFlingers.Entities
{
    public interface IInteractable
    {
        Transform transform { get; }
        InteractHotkey Hotkey { get; }
        bool CanPrompt();
        WorldUI CreatePromptUI();
        void Interact();
    }
}