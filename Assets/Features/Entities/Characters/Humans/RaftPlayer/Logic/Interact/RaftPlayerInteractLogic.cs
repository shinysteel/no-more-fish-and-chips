using NUnit.Framework;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;
using System.Collections.Generic;
using NoMoreFishAndChips.UI;
using ShinyOwl.Common.Framework;
using System;
using System.Linq;
using UnityEngine.Pool;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Interactions;
using ShinyOwl.Common.Utils;
using NoMoreFishAndChips.Rendering;

using Object = UnityEngine.Object;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerInteractLogic : RaftPlayerLogic
    {
        private UIManager _uiManager;
        private RenderingManager _renderingManager;

        private RaftPlayerInteractSharedSettings _sharedSettings;

        private float _animateTimer;

        private List<NearbyInteractable> _nearbyInteractables = new();

        private IInteractable _promptInteractable;
        private List<Interaction> _promptInteractions = new();
        private InteractPromptsUI _promptsUI;
        private OutlineHandle _promptOutlineHandle;

        private Collider[] _interactableCollidersNonAlloc = new Collider[MaxOverlaps];
        private const int MaxOverlaps = 20;

        // An interactable that can be considered 'nearby', meaning its angle and distance relative to the player are below the maxes
        private class NearbyInteractable : IComparable<NearbyInteractable>
        {
            private IInteractable _interactable;
            private Collider _collider;
            private int _priority;
            private float _angle;
            private float _distance;

            public IInteractable Interactable => _interactable;
            public Collider Collider => _collider;

            public NearbyInteractable(IInteractable interactable, Collider collider)
            {
                _interactable = interactable;
                _collider = collider;
            }

            public void Set(int priority, float angle, float distance)
            {
                _priority = priority;
                _angle = angle;
                _distance = distance;
            }

            public int CompareTo(NearbyInteractable other)
            {
                int priorityCompare = other._priority.CompareTo(_priority);
                if (priorityCompare != 0)
                {
                    return priorityCompare;
                }

                int angleCompare = _angle.CompareTo(other._angle);
                if (angleCompare != 0)
                {
                    return angleCompare;
                }

                return _distance.CompareTo(other._distance);
            }
        }

        public RaftPlayerInteractLogic(RaftPlayer player) : base(player)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
            _renderingManager = GameManager.Instance.Get<RenderingManager>();

            _sharedSettings = _player.DefinitionData.InteractSharedSettings;
        }

        public override void OnDespawned()
        {
            ClearPrompts();
        }

        private void ClearPrompts()
        {
            if (_promptsUI != null)
            {
                _uiManager.DestroyWorldUI(_promptsUI);
            }

            if (_promptInteractable != null)
            {
                foreach (Interaction interaction in _promptInteractable.Interactions)
                {
                    interaction.HidePreview();
                }
            }

            _promptOutlineHandle?.Remove();

            _promptsUI = null;
            _promptInteractable = null;
            _promptOutlineHandle = null;
        }
        
        public void Interact(ActionHotkey hotkey)
        {
            Interaction interaction = _promptInteractable.Interactions.FirstOrDefault(interaction => interaction.Hotkey == hotkey);

            if (interaction?.CanInteract() ?? false)
            {
                interaction.Interact();
            }
        }

        public override void Tick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            NullTick();
            AddTick();

            List<NearbyInteractable> elementsToRemove = ListPool<NearbyInteractable>.Get();

            RecalculateTick(elementsToRemove);
            RemoveTick(elementsToRemove);

            ListPool<NearbyInteractable>.Release(elementsToRemove);

            _nearbyInteractables.Sort();

            PromptTick();
            AnimateTick();
        }

        private void NullTick()
        {
            // Active interactables can become null while they are being prompted, such as a DroppedItem
            _nearbyInteractables.RemoveAll(nearbyInteractable => nearbyInteractable.Interactable as Object == null);
        }

        private void AddTick()
        {
            if (!_player.CharacterActLogic.CanAct)
            {
                return;
            }

            int overlaps = Physics.OverlapSphereNonAlloc(_player.transform.position, _sharedSettings.Radius, _interactableCollidersNonAlloc, _sharedSettings.Mask);

            // Track new interactables that match our hotkey and are nearby
            for (int i = 0; i < overlaps; i++)
            {
                if (!Utils.Physics.ColliderTryGetComponent(_interactableCollidersNonAlloc[i], out IInteractable interactable))
                {
                    continue;
                }

                if (_nearbyInteractables.Any(nearbyInteractable => nearbyInteractable.Collider == _interactableCollidersNonAlloc[i]))
                {
                    continue;
                }

                if (!CanPrompt(interactable, _interactableCollidersNonAlloc[i], out _, out _))
                {
                    continue;
                }

                _nearbyInteractables.Add(new NearbyInteractable(interactable, _interactableCollidersNonAlloc[i]));
            }
        }

        private void RecalculateTick(List<NearbyInteractable> elementsToRemove)
        {
            if (!_player.CharacterActLogic.CanAct)
            {
                elementsToRemove.AddRange(_nearbyInteractables);
                return;
            }
            
            // Recalculate angles and distances, and remove any that are no longer 'nearby'
            foreach (NearbyInteractable nearbyInteractable in _nearbyInteractables)
            {
                if (CanPrompt(nearbyInteractable.Interactable, nearbyInteractable.Collider, out float angle, out float distance))
                {
                    nearbyInteractable.Set(nearbyInteractable.Interactable.IInteractableSettings.Priority, angle, distance);
                }
                else
                {
                    elementsToRemove.Add(nearbyInteractable);
                }
            }
        }

        private void RemoveTick(List<NearbyInteractable> elementsToRemove)
        {
            foreach (NearbyInteractable remove in elementsToRemove)
            {
                _nearbyInteractables.Remove(remove);
            }
        }

        private void PromptTick()
        {
            // Reevaluate what UI we are showing and animate it
            if (_nearbyInteractables.Count == 0 || _promptInteractable != _nearbyInteractables[0].Interactable)
            {
                ClearPrompts();
            }

            if (_nearbyInteractables.Count > 0)
            {
                if (_promptInteractable == null)
                {
                    _promptInteractable = _nearbyInteractables[0].Interactable;
                    _promptsUI = _uiManager.CreateWorldUI(_uiManager.Config.InteractPromptsUIPrefab, Vector3.zero);

                    foreach (Interaction interaction in _promptInteractable.Interactions)
                    {
                        interaction.ShowPreview();
                        interaction.SetPreviewColor(interaction.CanInteract() ? _sharedSettings.ValidColor : _sharedSettings.InvalidColor);
                    }

                    if (_promptInteractable.Renderers != null)
                    {
                        _promptOutlineHandle = _renderingManager.CreateOutline(_promptInteractable.Renderers);
                    }
                }

                _promptInteractions.Clear();

                foreach (Interaction interaction in _promptInteractable.Interactions)
                {
                    if (interaction.CanPrompt())
                    {
                        _promptInteractions.Add(interaction);
                    }
                }

                _promptsUI.SetInteractionsNonAlloc(_promptInteractions);
            }
        }

        private void AnimateTick()
        {
            _animateTimer += Time.deltaTime;

            if (_promptsUI == null)
            {
                return;
            }

            float amplitude = 0.05f;
            float frequency = 2f;
            float phaseShift = 0f;
            float verticalShift = 0.5f;
            Vector3 offset = Vector3.up * (amplitude * Mathf.Sin(frequency * (_animateTimer - phaseShift)) + verticalShift);

            // Bob up and down slightly above the interactable
            _promptsUI.transform.position = _promptInteractable.Position + offset;
        }

        private bool CanPrompt(IInteractable interactable, Collider collider, out float angle, out float distance)
        {
            angle = 0f;
            distance = 0f;

            if (_player.BuildLogic.IsBuilding)
            {
                return false;
            }

            if (interactable.Interactions.All(interaction => !interaction.CanPrompt()))
            {
                return false;
            }

            Vector3 direction = (collider.transform.position - _player.transform.position);
            direction.y = 0f;
            direction.Normalize();
            angle = Vector3.Angle(_player.transform.forward, direction);

            distance = Vector3.Distance(_player.transform.position, collider.transform.position);

            if (distance > _sharedSettings.Radius)
            {
                return false;
            }

            return angle < interactable.IInteractableSettings.MaxAngle || distance < interactable.IInteractableSettings.MaxDistance;
        }
    }
}