using NoMoreFishAndChips.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips
{
    public interface IInteractable
    {
        Vector3 Position { get; }
        IEnumerable<Renderer> Renderers { get => null; }
        IInteractableSettings IInteractableSettings { get; }
        List<Interaction> Interactions { get; }
    }
}