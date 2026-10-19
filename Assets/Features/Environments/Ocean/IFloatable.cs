using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public interface IFloatable
    {
        Rigidbody Rigidbody { get; }
        bool CanFloat();
    }
}