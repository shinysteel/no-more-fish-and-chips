using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public interface IBuildableData : ICreatableData
    {
        EntityDefinitionData EntityDefinitionData { get; }
    }
}