using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Localisation;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class EntityDefinitionData : DefinitionData
    {
        [SerializeField] protected EntityId _id;
        [SerializeField] private EntityAlliance _alliance;
        [SerializeField] private SurfaceType _surfaceType;
        [SerializeField] private DropTable[] _dropTables;
        [SerializeField] protected EntityDefeatSharedSettings _entityDefeatSharedSettings;
        [SerializeField] private EntityLifecycleSharedSettings _entityLifecycleSharedSettings;
        [SerializeField] protected EntityPhysicsSharedSettings _entityPhysicsSharedSettings;
        [SerializeField] private EntityHealthSettings _entityHealthSettings;

        public EntityId Id => _id;
        public EntityAlliance Alliance => _alliance;
        public SurfaceType SurfaceType => _surfaceType;
        public DropTable[] DropTables => _dropTables;
        public EntityDefeatSharedSettings EntityDefeatSharedSettings => _entityDefeatSharedSettings;
        public EntityLifecycleSharedSettings EntityLifecycleSharedSettings => _entityLifecycleSharedSettings;
        public EntityPhysicsSharedSettings EntityPhysicsSharedSettings => _entityPhysicsSharedSettings;
        public EntityHealthSettings EntityHealthSettings => _entityHealthSettings;
    }
}