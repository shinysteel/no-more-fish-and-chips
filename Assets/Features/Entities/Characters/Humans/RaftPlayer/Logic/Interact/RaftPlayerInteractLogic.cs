using NUnit.Framework;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;
using System.Collections.Generic;
using FishFlingers.UI;
using ShinyOwl.Common.Framework;
using System;

using Object = UnityEngine.Object;
using System.Linq;
using UnityEngine.Pool;

namespace FishFlingers.Entities
{
    public class RaftPlayerInteractLogic
    {
        private UIManager _uiManager;

        private RaftPlayer _player;

        private RaftPlayerInteractSettings _settings;

        private float _animateTimer;

        private Dictionary<InteractHotkey, InteractView> _hotkeyViewMap = new();

        // Not to be confused with 'NearbyInteractable'. An overlapping interactable is simply an interactable within range of a Physics.OverlapSphere
        private List<IInteractable> _overlappingInteractables = new();

        private Collider[] _collidersNonAlloc = new Collider[MaxOverlaps];
        private const int MaxOverlaps = 10;

        // Manages a collection of nearby interactables for a hotkey, and displays a promptUI for the nearest interactable
        private class InteractView
        {
            private InteractHotkey _hotkey;
            private RaftPlayerInteractLogic _logic;

            private List<NearbyInteractable> _nearbyInteractables = new();

            private IInteractable _promptInteractable;
            private WorldUI _promptUI;

            public IInteractable PromptInteractable => _promptInteractable;

            public InteractView(InteractHotkey hotkey, RaftPlayerInteractLogic logic)
            {
                _hotkey = hotkey;
                _logic = logic;
            }

            public void Cleanup()
            {
                if (_promptUI != null)
                {
                    _logic._uiManager.DestroyWorldUI(_promptUI);
                }

                _promptUI = null;
                _promptInteractable = null;
            }

            public void Tick()
            {
                NullTick();
                AddTick();
                
                List<NearbyInteractable> elementsToRemove = ListPool<NearbyInteractable>.Get();

                RecalculateTick(elementsToRemove);
                RemoveTick(elementsToRemove);

                ListPool<NearbyInteractable>.Release(elementsToRemove);
                
                _nearbyInteractables.Sort();

                PromptTick();

                if (_promptUI != null)
                {
                    AnimateTick();
                }
            }

            private void NullTick()
            {
                // Active interactables can become null while they are being prompted, such as a DroppedItem
                _nearbyInteractables.RemoveAll(nearbyInteractable => nearbyInteractable.Interactable as Object == null);
            }

            private void AddTick()
            {
                // Track new interactables that match our hotkey and are nearby
                foreach (IInteractable interactable in _logic._overlappingInteractables)
                {
                    if (interactable.Hotkey != _hotkey)
                    {
                        continue;
                    }

                    if (_nearbyInteractables.Any(nearbyInteractable => nearbyInteractable.Interactable == interactable))
                    {
                        continue;
                    }

                    if (!interactable.CanPrompt())
                    {
                        continue;
                    }

                    GetAngleAndDistance(interactable, out float angle, out float distance);

                    if (!IsNearby(angle, distance))
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
                    GetAngleAndDistance(nearbyInteractable.Interactable, out float angle, out float distance);

                    if (nearbyInteractable.Interactable.CanPrompt() && IsNearby(angle, distance))
                    {
                        nearbyInteractable.Set(angle, distance);
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
                    }
                }
            }

            private void AnimateTick()
            {
                float amplitude = 0.05f;
                float frequency = 2f;
                float phaseShift = 0f;
                float verticalShift = 0.5f;
                Vector3 offset = Vector3.up * (amplitude * Mathf.Sin(frequency * (_logic._animateTimer - phaseShift)) + verticalShift);

                // Bob up and down slightly above the interactable
                _promptUI.transform.position = _promptInteractable.transform.position + offset;
            }

            private void GetAngleAndDistance(IInteractable interactable, out float angle, out float distance)
            {
                Vector3 direction = (interactable.transform.position - _logic._player.transform.position);
                direction.y = 0f;
                direction.Normalize();

                angle = Vector3.Angle(_logic._player.transform.forward, direction);
                distance = Vector3.Distance(_logic._player.transform.position, interactable.transform.position);
            }

            private bool IsNearby(float angle, float distance)
            {
                return angle < _logic._settings.MaxAngle || distance < _logic._settings.MaxDistance;
            }
        }

        // An interactable that can be considered 'nearby', meaning its angle and distance relative to the player are below the maxes
        private class NearbyInteractable : IComparable<NearbyInteractable>
        {
            private IInteractable _interactable;
            private float _angle;
            private float _distance;

            public IInteractable Interactable => _interactable;

            public NearbyInteractable(IInteractable interactable)
            {
                _interactable = interactable;
            }

            public void Set(float angle, float distance)
            {
                _angle = angle;
                _distance = distance;
            }

            public int CompareTo(NearbyInteractable other)
            {
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

            _player = player;

            _settings = _player.DefinitionData.InteractSettings;

            foreach (InteractHotkey hotkey in Enum.GetValues(typeof(InteractHotkey)))
            {
                if (hotkey == InteractHotkey.None)
                {
                    continue;
                }

                _hotkeyViewMap.Add(hotkey, new InteractView(hotkey, this));
            }
        }

        public void Cleanup()
        {
            foreach (InteractView view in _hotkeyViewMap.Values)
            {
                view.Cleanup();
            }

            _hotkeyViewMap.Clear();
        }
        
        public void Interact(InteractHotkey hotkey)
        {
            if (!_hotkeyViewMap.TryGetValue(hotkey, out InteractView view))
            {
                return;
            }

            view.PromptInteractable?.Interact();
        }

        public void Tick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            _animateTimer += Time.deltaTime;

            OverlapTick();
            ViewTick();
        }

        private void OverlapTick()
        {
            // Refresh overlapping interactables
            int overlaps = Physics.OverlapSphereNonAlloc(_player.transform.position, _settings.Radius, _collidersNonAlloc, _settings.Mask);

            _overlappingInteractables.Clear();

            for (int i = 0; i < overlaps; i++)
            {
                if (_collidersNonAlloc[i].TryGetComponent(out IInteractable interactable))
                {
                    _overlappingInteractables.Add(interactable);
                }
            }
        }

        private void ViewTick()
        {
            // Tick all views
            foreach (InteractView view in _hotkeyViewMap.Values)
            {
                view.Tick();
            }
        }
    }
}