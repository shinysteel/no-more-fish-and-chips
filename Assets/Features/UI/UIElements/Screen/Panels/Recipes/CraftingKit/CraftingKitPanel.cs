using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class CraftingKitPanel : RecipesPanel<ICraftable>
    {
        protected override IEnumerable<ICraftable> GetCreatables()
        {
            return _itemManager.GetAllItemDefinitionDatas().Where(data => data.BuildRecipe?.Requirements?.Length > 0);
        }

        protected override void CreatePressed(ICraftable craftable)
        {
            craftable.TryCraft(_context);
        }
    }
}