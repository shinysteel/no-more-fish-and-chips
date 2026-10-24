using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.States;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftTileDefinitionData", menuName = "Data/Entities/RaftTileDefinitionData")]
    public class RaftTileDefinitionData : EntityDefinitionData, IBuildableData
    {
        [SerializeField] private Recipe _buildRecipe;
        [SerializeField] private IInteractableSettings _iInteractableSettings;
        [SerializeField] private float _bobSpeed = 0.25f;
        [SerializeField] private Color _damagedColor;
        [SerializeField] private Recipe _repairRecipe;
        [SerializeField] private bool _isScaffold;

        public Recipe BuildRecipe => _buildRecipe;
        public IInteractableSettings IInteractableSettings => _iInteractableSettings;
        public float BobSpeed => _bobSpeed;
        public Color DamagedColor => _damagedColor;
        public Recipe RepairRecipe => _repairRecipe;
        public bool IsScaffold => _isScaffold;

        public RaftTileDefeatSharedSettings TileDefeatSharedSettings => (RaftTileDefeatSharedSettings)_entityDefeatSharedSettings;

        DefinitionData ICreatableData.DefinitionData => this;
        EntityDefinitionData IBuildableData.EntityDefinitionData => this;
    }
}