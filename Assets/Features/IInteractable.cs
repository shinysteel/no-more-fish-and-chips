using NoMoreFishAndChips.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips
{
    public interface IInteractable
    {
        Vector3 Position { get; }
        IInteractableSettings IInteractableSettings { get; }
        bool CanPrompt();
        WorldUI CreatePromptUI();
        bool CanInteract();
        void Interact();

        IEnumerable<Renderer> Renderers { get => null; }
        void ShowPreview() { }
        void SetPreviewColor(Color color) { }
        void HidePreview() { }
    }
}