using PurrNet;
using ShinyOwl.Common;
using UnityEngine;
using NoMoreFishAndChips.Environments;
using NUnit.Framework;
using System.Collections.Generic;

namespace NoMoreFishAndChips.Entities
{
    public class StructureScaffold : Structure<StructureScaffoldDefinitionData>
    {
        private SyncVar<EntityId> _netBuildId = new SyncVar<EntityId>(ownerAuth: true);
        private SyncVar<int> _netBuildRotations = new SyncVar<int>(ownerAuth: true);

        private List<Prop> _tapeProps = new();
        private List<Prop> _standardProps = new();

        protected override void OnSpawned()
        {
            base.OnSpawned();

            HandleNetBuildRotationsChanged(_netBuildRotations.value);

            _netBuildRotations.onChanged += HandleNetBuildRotationsChanged;

            Test();
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

        private void Test()
        {
            foreach (Prop prop in _tapeProps)
            {
                _environmentManager.ReturnProp(prop);
            }

            _tapeProps.Clear();

            _shape.ForEachTrue((Vector2Int cell) =>
            {
                void processSide(Vector3 direction)
                {
                    Vector2Int offset = new Vector2Int((int)direction.x, (int)direction.z);
                    Vector3 position = new Vector3(cell.x, 0f, cell.y) * 0.5f + direction * 0.25f;
                    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

                    if (!_shape.TryGetBool(cell + offset, out bool value) || !value)
                    {
                        Prop tape = _environmentManager.GetProp(PropId.ScaffoldTape, new SpawnParams() { Position = position, Rotation = rotation, Parent = transform });
                        _tapeProps.Add(tape);
                    }
                }

                processSide(Vector3.forward);
                processSide(Vector3.right);
                processSide(Vector3.back);
                processSide(Vector3.left);
            });
        }
    }
}