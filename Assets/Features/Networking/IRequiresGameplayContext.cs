using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Networking
{
    public interface IRequiresGameplayContext
    {
        bool IsContextInitialised { get; }

        void InitialiseContext(GameplayContext context);
    }
}