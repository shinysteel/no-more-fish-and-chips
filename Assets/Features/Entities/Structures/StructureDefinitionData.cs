using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.States;
using UnityEngine;
using ShinyOwl.Common;

namespace NoMoreFishAndChips.Entities
{
    public abstract class StructureDefinitionData : EntityDefinitionData, IBuildable
    {
        [SerializeField] private Recipe _buildRecipe;

        public Recipe BuildRecipe => _buildRecipe;

        DefinitionData ICreatable.DefinitionData => this;
        EntityDefinitionData IBuildable.EntityDefinitionData => this;
    }
}