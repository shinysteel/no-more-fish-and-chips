using UnityEngine;

namespace FishFlingers.Entities
{
    public interface IInteractable
    {
        Vector3 Position { get; }
        void Interact();
    }
}