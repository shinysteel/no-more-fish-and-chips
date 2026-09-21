using PurrNet;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class StructureScaffold : Structure<StructureScaffoldDefinitionData>
    {
        private SyncVar<EntityId> _netBuildId = new SyncVar<EntityId>(ownerAuth: true);
        private SyncVar<int> _netBuildRotations = new SyncVar<int>(ownerAuth: true);

        protected override void OnSpawned()
        {
            base.OnSpawned();

            HandleNetBuildRotationsChanged(_netBuildRotations.value);

            _netBuildRotations.onChanged += HandleNetBuildRotationsChanged;
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _netBuildRotations.onChanged -= HandleNetBuildRotationsChanged;
        }

        protected override void RefreshShape()
        {
            if (_netBuildId.value == EntityId.None)
            {
                return;
            }
           
            Structure prefab = (Structure)_entityManager.GetPrefab(_netBuildId.value);

            _shape = prefab.StructureDefinitionData.Shape.GetTransformed(Vector2Int.zero, _netBuildRotations.value);
        }

        private void HandleNetBuildRotationsChanged(int rotations)
        {
            RefreshShape();
        }

        public void SetNetBuildId(EntityId id)
        {
            _netBuildId.value = id;
        }

        public void SetNetBuildRotations(int rotations)
        {
            _netBuildRotations.value = rotations;

            if (isOwner)
            {
                HandleNetBuildRotationsChanged(_netBuildRotations.value);
            }
        }
    }
}