using PurrNet;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class StructureScaffold : Structure<StructureScaffoldDefinitionData>
    {
        private SyncVar<EntityId> _netBuildId = new SyncVar<EntityId>(ownerAuth: true);
        private SyncVar<int> _netBuildRotations = new SyncVar<int>(ownerAuth: true);

        public void SetNetBuildId(EntityId id)
        {
            _netBuildId.value = id;
        }

        public void SetNetBuildRotations(int rotations)
        {
            _netBuildRotations.value = rotations;
        }
    }
}