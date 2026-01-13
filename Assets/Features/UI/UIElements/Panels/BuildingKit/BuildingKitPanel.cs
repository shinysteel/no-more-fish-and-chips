using FishFlingers.Entities;
using FishFlingers.Pools;
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

        public override void Load()
        {
            base.Load();

            _entityManager = GameManager.Instance.Get<EntityManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();

            foreach (Structure structure in _entityManager.GetEntities<Structure>())
            {
                BlueprintEntry entry = _poolManager.Get<BlueprintEntry>(new SpawnParams() { Parent = _blueprintsScrollRect.content });
                entry.Setup(structure.StructureData);
            }
        }
    }
}