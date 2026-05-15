using UnityEngine;

namespace FishFlingers.Entities
{
    public interface IInteractable
    {
        Transform transform { get; }
        void Interact();
    }
}