using NoMoreFishAndChips;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Items
{
    public interface ICraftable : ICreatable
    {
        bool TryCraft(GameplayContext context);
    }
}