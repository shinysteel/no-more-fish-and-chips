using NoMoreFishAndChips.Items;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using ShinyOwl.Common.Utils;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class RequirementPrompt : InteractPrompt
    {
        [SerializeField] private Transform _itemsContainer;

        private PoolManager _poolManager;

        private List<RequirementPromptItem> _items = new();

        protected override void Awake()
        {
            base.Awake();

            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public void SetupRequirement(GameplayContext context, Recipe recipe)
        {
            Utils.Collections.ResizeList(_items, recipe.Requirements.Length,
                createElement: () => _poolManager.GetTypedPoolable<RequirementPromptItem>(new SpawnParams() { Parent = _itemsContainer }),
                removeElement: (RequirementPromptItem item) => _poolManager.ReturnTypedPoolable(item),
                processElement: (RequirementPromptItem item, int index) => item.Setup(context, recipe.Requirements[index]));

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }
    }
}