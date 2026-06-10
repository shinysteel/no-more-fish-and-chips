using NoMoreFishAndChips.Items;
using UnityEngine;

namespace NoMoreFishAndChips
{
    public interface ICreatable
    {
        DefinitionData DefinitionData { get; }
        Recipe BuildRecipe { get; }
    }
}