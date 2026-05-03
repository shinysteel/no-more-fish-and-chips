using FishFlingers.Items;
using FishFlingers.States;
using UnityEngine;

namespace FishFlingers.Entities
{
    public interface IBuildable : ICreatable
    {
        bool TryBuild(GameplayContext context, RaftPlayerTileTarget target);
    }
}