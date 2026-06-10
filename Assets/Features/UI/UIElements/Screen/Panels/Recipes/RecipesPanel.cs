using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using ShinyOwl.Common.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using NoMoreFishAndChips.Items;

namespace NoMoreFishAndChips.UI
{
    public abstract class RecipesPanel<T> : Panel where T : ICreatable
    {
        [SerializeField] private ScrollRect _blueprintsScrollRect;

        protected EntityManager _entityManager;
        protected PoolManager _poolManager;
        protected ItemManager _itemManager;

        protected GameplayContext _context;

        private List<BlueprintEntry> _blueprintEntries = new();

        public override void Load(Canvas canvas)
        {
            base.Load(canvas);

            _entityManager = GameManager.Instance.Get<EntityManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();
            _itemManager = GameManager.Instance.Get<ItemManager>();
        }

        public virtual void Setup(GameplayContext context)
        {
            _context = context;

            RefreshEntries();
        }

        public override void Unload()
        {
            foreach (BlueprintEntry entry in _blueprintEntries)
            {
                _poolManager.ReturnTypedPoolable(entry);
            }
        }

        protected void RefreshEntries()
        {
            IEnumerable<T> creatables = GetCreatables();

            Utils.Collections.ResizeList(_blueprintEntries, creatables.Count(),
                createElement: () => _poolManager.GetTypedPoolable<BlueprintEntry>(new SpawnParams() { Parent = _blueprintsScrollRect.content }),
                removeElement: (BlueprintEntry entry) => _poolManager.ReturnTypedPoolable(entry),
                processElement: processElement);

            void processElement(BlueprintEntry entry, int index)
            {
                T creatable = creatables.ElementAt(index);
                entry.Setup(creatable, () => CreatePressed(creatable));
            }
        }

        protected abstract IEnumerable<T> GetCreatables();
        protected abstract void CreatePressed(T creatable);
    }
}