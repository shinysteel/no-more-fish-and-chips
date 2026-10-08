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
        [SerializeField] private float _buildTime = 1f;
        [SerializeField] private bool _isScaffold;

        public BoolGrid Shape => _shape;
        public Recipe BuildRecipe => _buildRecipe;
        public float BuildTime => _buildTime;
        public bool IsScaffold => _isScaffold;

        DefinitionData ICreatable.DefinitionData => this;
        EntityDefinitionData IBuildable.EntityDefinitionData => this;
    }
}