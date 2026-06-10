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

using Object = UnityEngine.Object;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerInteractLogic
    {
        private UIManager _uiManager;
        private EnvironmentManager _environmentManager;

        private RaftPlayer _player;

        private RaftPlayerInteractSettings _settings;

        private float _animateTimer;

        private List<NearbyInteractable> _nearbyInteractables = new();

        private IInteractable _promptInteractable;
        private WorldUI _promptUI;
        private Prop _promptPreview;

        private Collider[] _collidersNonAlloc = new Collider[MaxOverlaps];
        private const int MaxOverlaps = 10;

        // An interactable that can be considered 'nearby', meaning its angle and distance relative to the player are below the maxes
        private class NearbyInteractable : IComparable<NearbyInteractable>
        {
            private IInteractable _interactable;
            private int _priority;
            private float _angle;
            private float _distance;

            public IInteractable Interactable => _interactable;

            public NearbyInteractable(IInteractable interactable)
            {
                _interactable = interactable;
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

        public RaftPlayerInteractLogic(RaftPlayer player)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
            _environmentManager = GameManager.Instance.Get<EnvironmentManager>();

            _player = player;

            _settings = _player.DefinitionData.InteractSettings;
        }

        public void Cleanup()
        {
            if (_promptUI != null)
            {
                _uiManager.DestroyWorldUI(_promptUI);
            }

            if (_promptPreview != null)
            {
                _environmentManager.ReturnProp(_promptPreview);
            }

            _promptUI = null;
            _promptPreview = null;
            _promptInteractable = null;
        }
        
        public void Interact(ActionHotkey hotkey)
        {
            if (_promptInteractable?.Settings.Hotkey != hotkey)
            {
                return;
            }

            _promptInteractable.Interact();
        }

        public void Tick()
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
            int overlaps = Physics.OverlapSphereNonAlloc(_player.transform.position, _settings.Radius, _collidersNonAlloc, _settings.Mask);

            // Track new interactables that match our hotkey and are nearby
            for (int i = 0; i < overlaps; i++)
            {
                if (!_collidersNonAlloc[i].TryGetComponent(out IInteractable interactable))
                {
                    continue;
                }

                if (_nearbyInteractables.Any(nearbyInteractable => nearbyInteractable.Interactable == interactable))
                {
                    continue;
                }

                if (!CanPrompt(interactable, out _, out _))
                {
                    continue;
                }

                _nearbyInteractables.Add(new NearbyInteractable(interactable));
            }
        }

        private void RecalculateTick(List<NearbyInteractable> elementsToRemove)
        {
            // Recalculate angles and distances, and remove any that are no longer 'nearby'
            foreach (NearbyInteractable nearbyInteractable in _nearbyInteractables)
            {
                if (CanPrompt(nearbyInteractable.Interactable, out float angle, out float distance))
                {
                    nearbyInteractable.Set(nearbyInteractable.Interactable.Settings.Priority, angle, distance);
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
                Cleanup();
            }

            if (_nearbyInteractables.Count > 0)
            {
                if (_promptInteractable == null)
                {
                    _promptInteractable = _nearbyInteractables[0].Interactable;
                    _promptUI = _promptInteractable.CreatePromptUI();

                    if (_promptInteractable.Settings.PreviewId != PropId.None)
                    {
                        _promptPreview = _environmentManager.GetProp(_promptInteractable.Settings.PreviewId, new SpawnParams() { Parent = _promptInteractable.transform, Position = _promptInteractable.Settings.PreviewPosition });
                        RefreshPreviewColor();
                    }
                }
            }
        }

        private void RefreshPreviewColor()
        {
            _promptPreview.SetColor(_promptInteractable.CanInteract() ? _settings.ValidColor : _settings.InvalidColor);
        }

        private void AnimateTick()
        {
            _animateTimer += Time.deltaTime;

            if (_promptUI == null)
            {
                return;
            }

            float amplitude = 0.05f;
            float frequency = 2f;
            float phaseShift = 0f;
            float verticalShift = 0.5f;
            Vector3 offset = Vector3.up * (amplitude * Mathf.Sin(frequency * (_animateTimer - phaseShift)) + verticalShift);

            // Bob up and down slightly above the interactable
            _promptUI.transform.position = _promptInteractable.transform.position + offset;
        }

        private bool CanPrompt(IInteractable interactable, out float angle, out float distance)
        {
            angle = 0f;
            distance = 0f;

            if (_player.TileTargetLogic.IsBuilding)
            {
                return false;
            }

            if (!interactable.CanPrompt())
            {
                return false;
            }

            Vector3 direction = (interactable.transform.position - _player.transform.position);
            direction.y = 0f;
            direction.Normalize();
            angle = Vector3.Angle(_player.transform.forward, direction);

            distance = Vector3.Distance(_player.transform.position, interactable.transform.position);

            if (distance > _settings.Radius)
            {
                return false;
            }

            return angle < interactable.Settings.MaxAngle || distance < interactable.Settings.MaxDistance;
        }
    }
}