using FishFlingers.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FishFlingers.UI
{
    public class CraftingKitPanel : RecipesPanel<ICraftable>
    {
        protected override IEnumerable<ICraftable> GetCreatables()
        {
            return _itemManager.GetAllItemDefinitionDatas().Where(data => data.Recipe?.Requirements?.Length > 0);
        }

        protected override void CreatePressed(ICraftable craftable)
        {
            craftable.TryCraft(_context);
        }
    }
}