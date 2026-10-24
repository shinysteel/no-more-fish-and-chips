using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.States;
using UnityEngine;
using ShinyOwl.Common;
using ShinyOwl.Common.Structures;

namespace NoMoreFishAndChips.Entities
{
    public abstract class StructureDefinitionData : EntityDefinitionData, IBuildableData
    {
        [SerializeField] private BoolGrid _shape;
        [SerializeField] private BoolGrid _waterMask;
        [SerializeField] private bool _enforceWaterMask;
        [SerializeField] private Recipe _buildRecipe;
        [SerializeField] private bool _isScaffold;

        public BoolGrid Shape => _shape;
        public BoolGrid WaterMask => _waterMask;
        public bool EnforceWaterMask => _enforceWaterMask;
        public Recipe BuildRecipe => _buildRecipe;
        public bool IsScaffold => _isScaffold;

        DefinitionData ICreatableData.DefinitionData => this;
        EntityDefinitionData IBuildableData.EntityDefinitionData => this;
    }
}