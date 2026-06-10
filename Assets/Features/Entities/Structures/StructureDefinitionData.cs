using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.States;
using UnityEngine;
using ShinyOwl.Common;

namespace NoMoreFishAndChips.Entities
{
    public abstract class StructureDefinitionData : EntityDefinitionData, IBuildable
    {
        [SerializeField] private Recipe _buildRecipe;

        public DefinitionData DefinitionData => this;
        public Recipe BuildRecipe => _buildRecipe;

        public bool TryBuild(GameplayContext context, RaftPlayerTileTarget target)
        {
            if (!target.CanBuildStructure())
            {
                return false;
            }

            context.Raft.AddStructureRpc(target.Cell, _id);

            return true;
        }
    }
}