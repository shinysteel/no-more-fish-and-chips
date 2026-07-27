using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public interface IBuildable : ICreatable
    {
        bool TryBuild(GameplayContext context, RaftTileTarget target);
    }
}