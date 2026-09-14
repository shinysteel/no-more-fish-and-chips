using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.States;
using UnityEngine;
using ShinyOwl.Common;
using ShinyOwl.Common.Structures;

namespace NoMoreFishAndChips.Entities
{
    public abstract class StructureDefinitionData : EntityDefinitionData, IBuildable
    {
        [SerializeField] private BoolGrid _shape;
        [SerializeField] private Recipe _buildRecipe;

        public BoolGrid Shape => _shape;
        public Recipe BuildRecipe => _buildRecipe;

        DefinitionData ICreatable.DefinitionData => this;
        EntityDefinitionData IBuildable.EntityDefinitionData => this;
    }
}