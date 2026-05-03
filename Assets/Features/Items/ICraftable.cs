using FishFlingers;
using FishFlingers.Items;
using FishFlingers.States;
using UnityEngine;

namespace FishFlingers.Items
{
    public interface ICraftable : ICreatable
    {
        bool TryCraft(GameplayContext context);
    }
}