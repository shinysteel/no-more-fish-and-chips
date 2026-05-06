using FishFlingers.Items;
using FishFlingers.Localisation;
using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "EntityDefinitionData", menuName = "Data/Entities/EntityDefinitionData")]
    public class EntityDefinitionData : DefinitionData
    {
        [SerializeField] protected EntityId _id;
        [SerializeField] protected int _health = 1;
        [SerializeField] private bool _isDamageable = true;
        [SerializeField] private EntityAlliance _alliance;
        [SerializeField] private DropTable[] _dropTables;
        [SerializeField] private EntityDefeatSettings _entityDefeatSettings;
        [SerializeField] private EntityLifecycleSettings _entityLifecycleSettings;

        public EntityId Id => _id;
        public int Health => _health;
        public bool IsDamageable => _isDamageable;
        public EntityAlliance Alliance => _alliance;
        public DropTable[] DropTables => _dropTables;
        public EntityDefeatSettings EntityDefeatSettings => _entityDefeatSettings;
        public EntityLifecycleSettings EntityLifecycleSettings => _entityLifecycleSettings;
    }
}