using NoMoreFishAndChips.Items;
using UnityEngine;

namespace NoMoreFishAndChips
{
    public interface ICreatableData
    {
        DefinitionData DefinitionData { get; }
        Recipe BuildRecipe { get; }
    }
}