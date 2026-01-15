using System;
using UnityEngine;

namespace FishFlingers.Items
{
    [Serializable]
    public class RecipeRequirement
    {
        [SerializeField] private ItemId _itemId;
        [SerializeField] private int _count;

        public ItemId ItemId => _itemId;
        public int Count => _count;
    }

    [Serializable]
    public class Recipe
    {
        [SerializeField] private RecipeRequirement[] _requirements;

        public RecipeRequirement[] Requirements => _requirements;
    }
}