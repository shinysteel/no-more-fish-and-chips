using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public interface IBuildable : ICreatable
    {
        EntityDefinitionData EntityDefinitionData { get; }
    }
}