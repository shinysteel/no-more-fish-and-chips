using NoMoreFishAndChips;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Items
{
    public interface ICraftable : ICreatableData
    {
        void Craft(GameplayContext context);
    }
}