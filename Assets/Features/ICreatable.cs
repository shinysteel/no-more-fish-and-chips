using FishFlingers.Items;
using UnityEngine;

namespace FishFlingers
{
    public interface ICreatable
    {
        DefinitionData DefinitionData { get; }
        Recipe Recipe { get; }
    }
}