using FishFlingers.Entities;
using FishFlingers.Pools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class BuildingKitPanel : Panel
    {
        [SerializeField] private ScrollRect _blueprintsScrollRect;

        private EntityManager _entityManager;
        private PoolManager _poolManager;

        private BlueprintEntry[] _blueprintEntries;

        public override void Load()
        {
            base.Load();

            _entityManager = GameManager.Instance.Get<EntityManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();

            IEnumerable<Structure> structures = _entityManager.GetEntities<Structure>();
            _blueprintEntries = new BlueprintEntry[structures.Count()];

            // Populate the blueprint entries
            int i = 0;
            foreach (Structure structure in structures)
            {
                BlueprintEntry entry = _poolManager.Get<BlueprintEntry>(new SpawnParams() { Parent = _blueprintsScrollRect.content });
                entry.Setup(structure.StructureData);

                _blueprintEntries[i] = entry;
                i++;
            }
        }

        public override void Unload()
        {
            base.Unload();

            foreach (BlueprintEntry entry in _blueprintEntries)
            {
                _poolManager.Return(entry);
            }
        }
    }
}