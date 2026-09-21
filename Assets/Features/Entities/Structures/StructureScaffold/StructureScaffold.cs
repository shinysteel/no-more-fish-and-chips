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
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            ReturnProps();

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

            RefreshProps();
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

        private void ReturnProps()
        {
            foreach (Prop prop in _tapeProps)
            {
                _environmentManager.ReturnProp(prop);
            }

            _tapeProps.Clear();

            foreach (Prop prop in _standardProps)
            {
                _environmentManager.ReturnProp(prop);
            }

            _standardProps.Clear();
        }
        
        private void RefreshProps()
        {
            ReturnProps();

            _shape.ForEachTrue((Vector2Int cell) =>
            {
                void processSide(Vector3 direction)
                {
                    Vector2Int offset = new Vector2Int((int)direction.x, (int)direction.z);

                    if (!_shape.TryGetBool(cell + offset, out bool value) || !value)
                    {
                        Vector3 position = new Vector3(cell.x, 0f, cell.y) * 0.5f + direction * 0.25f;
                        
                        // position.x -= Mathf.Sign(position.x) * 0.05f;
                        // position.z -= Mathf.Sign(position.z) * 0.05f;

                        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

                        Prop tape = _environmentManager.GetProp(PropId.ScaffoldTape, new SpawnParams() { Position = position, Rotation = rotation, Parent = transform });
                        _tapeProps.Add(tape);
                    }
                }

                processSide(Vector3.forward);
                processSide(Vector3.right);
                processSide(Vector3.back);
                processSide(Vector3.left);
            });

            for (int x = _shape.TrueBounds.xMin; x <= _shape.TrueBounds.xMax; x++)
            {
                for (int y = _shape.TrueBounds.yMin; y <= _shape.TrueBounds.yMax; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    int count = 0;

                    void countCell(Vector2Int offset)
                    {
                        if (_shape.TryGetBool(cell + offset, out bool value) && value)
                        {
                            count++;
                        }
                    }

                    countCell(-Vector2Int.one);
                    countCell(Vector2Int.down);
                    countCell(Vector2Int.left);
                    countCell(Vector2Int.zero);

                    if (count != 1 && count != 3)
                    {
                        continue;
                    }

                    Vector3 position = new Vector3(cell.x * 0.5f - 0.25f, 0f, cell.y * 0.5f - 0.25f);

                    // position.x -= Mathf.Sign(position.x) * 0.05f;
                    // position.z -= Mathf.Sign(position.z) * 0.05f;

                    Prop standard = _environmentManager.GetProp(PropId.ScaffoldStandard, new SpawnParams() { Position = position, Rotation = Quaternion.LookRotation(Vector3.back, Vector3.up), Parent = transform });
                    _standardProps.Add(standard);
                }
            }
        }
    }
}