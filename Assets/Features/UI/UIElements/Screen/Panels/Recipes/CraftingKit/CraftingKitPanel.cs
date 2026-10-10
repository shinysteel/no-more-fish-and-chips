using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class CraftingKitPanel : RecipesPanel<ICraftable>
    {
        private ItemManager _itemManager;

        protected override void Awake()
        {
            base.Awake();

            _itemManager = GameManager.Instance.Get<ItemManager>();
        }

        protected override IEnumerable<ICraftable> GetCreatables()
        {
            return _itemManager.GetAllItemDefinitionDatas().Where(data => data.BuildRecipe?.Requirements?.Length > 0);
        }

        protected override void CreatePressed(ICraftable craftable)
        {
            craftable.Craft(_context);
        }
    }
}