using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.UI;
using NUnit.Framework;
using PurrNet;
using ShinyOwl.Common;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class StructureScaffold : Structure<StructureScaffoldDefinitionData>, IInteractable
    {
        private SyncVar<EntityId> _netBuildId = new SyncVar<EntityId>(ownerAuth: true);
        private SyncVar<int> _netBuildRotations = new SyncVar<int>(ownerAuth: true);

        private List<Prop> _tapeProps = new();
        private List<Prop> _standardProps = new();
        private List<Prop> _previewProps = new();
        private List<ColliderProxy> _colliderProxies = new();

        private Vector3 _iInteractablePositionOffset;
        Vector3 IInteractable.Position => transform.position + _iInteractablePositionOffset;
        IInteractableSettings IInteractable.IInteractableSettings => DefinitionData.IInteractableSettings;

        protected override void OnSpawned()
        {
            base.OnSpawned();

            HandleNetBuildRotationsChanged(_netBuildRotations.value);

            _netBuildRotations.onChanged += HandleNetBuildRotationsChanged;

            _shape.ForEachTrue((Vector2Int cell) =>
            {
                Vector3 position = new Vector3(cell.x, 0f, cell.y) * 0.5f + Vector3.up * 0.125f;
                Vector3 scale = new Vector3(0.5f, 0.25f, 0.5f);
                BoxColliderProxy proxy = _poolManager.GetTypedPoolable<BoxColliderProxy>(new SpawnParams() { Position = position, Scale = scale, Parent = transform });
                proxy.SetOwnerGameObject(gameObject);
                _colliderProxies.Add(proxy);
            });

            Vector3 position = _context.Raft.Queries.StructureCellToWorldPosition(_netCell.value + _shape.TrueBounds.center - Vector2.one * 0.5f);
            position.y = transform.position.y;
            _iInteractablePositionOffset = position - transform.position;
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            ReturnScaffoldProps();

            _netBuildRotations.onChanged -= HandleNetBuildRotationsChanged;

            ((IInteractable)this).HidePreview();

            foreach (ColliderProxy proxy in _colliderProxies)
            {
                _poolManager.ReturnTypedPoolable(proxy);
            }
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

        private void ReturnScaffoldProps()
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
            ReturnScaffoldProps();

            _shape.ForEachTrue((Vector2Int cell) =>
            {
                void processSide(Vector3 direction)
                {
                    Vector2Int offset = new Vector2Int((int)direction.x, (int)direction.z);

                    if (!_shape.TryGetBool(cell + offset, out bool value) || !value)
                    {
                        Vector3 position = new Vector3(cell.x, 0f, cell.y) * 0.5f + direction * 0.25f;
                        
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

                    Prop standard = _environmentManager.GetProp(PropId.ScaffoldStandard, new SpawnParams() { Position = position, Rotation = Quaternion.LookRotation(Vector3.back, Vector3.up), Parent = transform });
                    _standardProps.Add(standard);
                }
            }
        }

        bool IInteractable.CanPrompt()
        {
            return isSpawned && _context != null && _context.LocalPlayer.Hotbar.SelectedSlot.InventoryItem?.ItemInstance.Data.ItemId == ItemId.Hammer;
        }

        WorldUI IInteractable.CreatePromptUI()
        {
            InteractPromptUI ui = _uiManager.CreateWorldUI(_uiManager.Config.InteractPromptUIPrefab, Vector3.zero);
            ui.SetupInteract(DefinitionData.IInteractableSettings.Hotkey);
            return ui;
        }

        bool IInteractable.CanInteract()
        {
            return true;
        }

        void IInteractable.Interact()
        {
            Entity prefab = _entityManager.GetPrefab(_netBuildId.value);

            _context.Raft.SetStructureRpc(_netCell.value, _netBuildId.value, prefab.EntityDefinitionData.Health, _netBuildRotations.value);
        }

        void IInteractable.ShowPreview()
        {
            _shape.ForEachTrue((Vector2Int cell) =>
            {
                Vector3 position = new Vector3(cell.x, 0f, cell.y) * 0.5f + Vector3.up * 0.075f;
                Vector3 scale = new Vector3(0.51f, 0.15f, 0.51f);
                _previewProps.Add(_environmentManager.GetProp(PropId.BoxSelect, new SpawnParams() { Position = position, Scale = scale, Parent = transform }));
            });
        }

        void IInteractable.SetPreviewColor(Color color)
        { 
            foreach (Prop prop in _previewProps)
            {
                prop.SetColor(color);
            }
        }

        void IInteractable.HidePreview()
        { 
            foreach (Prop prop in _previewProps)
            {
                _environmentManager.ReturnProp(prop);
            }
        }
    }
}