using NUnit.Framework;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;
using System.Collections.Generic;
using FishFlingers.UI;
using ShinyOwl.Common.Framework;
using System;

using Object = UnityEngine.Object;

namespace FishFlingers.Entities
{
    public class InteractLogic
    {
        private UIManager _uiManager;

        private RaftPlayer _player;
        private InputLogic _inputLogic;

        private StateMachine<EState> _uiStateMachine;

        private InteractPromptUI _interactPromptUI;
        private float _animateTimer;

        private IInteractable _currentInteractable;
        private IInteractable _targetInteractable;

        private Collider[] _collidersNonAlloc = new Collider[MaxOverlaps];
        private List<NearbyInteractable> _nearbyInteractables = new List<NearbyInteractable>(MaxOverlaps);

        private const int MaxOverlaps = 10;

        private class NearbyInteractable : IComparable<NearbyInteractable>
        {
            public IInteractable Interactable { get; private set; }
            public float Distance { get; private set; }
            public float Angle { get; private set; }

            public NearbyInteractable(IInteractable interactable, float distance, float angle)
            {
                Interactable = interactable;
                Distance = distance;
                Angle = angle;
            }

            public int CompareTo(NearbyInteractable other)
            {
                int angleCompare = Angle.CompareTo(other.Angle);
                if (angleCompare != 0)
                {
                    return angleCompare;
                }

                return Distance.CompareTo(other.Distance);
            }
        }

        // Lifecycle of InteractPromptUI
        private enum EState
        {
            Idle     ,
            Create   ,
            Retarget ,
            Destroy  ,
        }

        private class State : State<EState, ENone>
        {
            protected InteractLogic _interactLogic;

            public State(StateMachine<EState> parent) : base(parent)
            { }

            public void Initialise(InteractLogic interactLogic)
            {
                _interactLogic = interactLogic;
            }
        }

        private class IdleState : State
        {
            public IdleState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Tick()
            {
                if (_interactLogic._targetInteractable != null)
                {
                    _parentStateMachine.ChangeState(EState.Create);
                }
            }
        }

        private class CreateState : State
        {
            public CreateState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                _interactLogic.SetCurrentInteractable(_interactLogic._targetInteractable);

                _interactLogic._interactPromptUI = _interactLogic._uiManager.CreateWorldUI(_interactLogic._uiManager.Config.InteractPromptUIPrefab, Vector3.zero);

                _interactLogic._interactPromptUI.Show(() =>
                {
                    if (_interactLogic._targetInteractable != null)
                    {
                        _parentStateMachine.ChangeState(EState.Retarget);
                    }
                    else
                    {
                        _parentStateMachine.ChangeState(EState.Destroy);
                    }
                });
            }
        }

        private class RetargetState : State
        {
            public RetargetState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Tick()
            {
                // Whenever the target switches, we need to hide the prompt first
                if (_interactLogic._currentInteractable != _interactLogic._targetInteractable)
                {
                    if (_interactLogic._interactPromptUI.IsShowing)
                    {
                        _interactLogic._interactPromptUI.Hide(() => _interactLogic.SetCurrentInteractable(_interactLogic._targetInteractable));
                    }

                    return;
                }

                // If the target switched to null, we need to destroy the prompt. Since it's
                // an interface, we need an Object cast to know if its null
                if (_interactLogic._targetInteractable as Object == null)
                {
                    _parentStateMachine.ChangeState(EState.Destroy);
                    return;
                }

                // Otherwise, retarget
                if (!_interactLogic._interactPromptUI.IsShowing)
                {
                    _interactLogic._interactPromptUI.Show(null);
                }
            }
        }

        private class DestroyState : State
        {
            public DestroyState(StateMachine<EState> parent) : base(parent)
            { }

            public override void Enter()
            {
                if (_interactLogic._interactPromptUI.IsShowing)
                {
                    _interactLogic._interactPromptUI.Hide(Destroy);
                }
                else
                {
                    Destroy();
                }
            }

            private void Destroy()
            {
                _interactLogic._uiManager.DestroyWorldUI(_interactLogic._interactPromptUI);
                _interactLogic._interactPromptUI = null;

                _interactLogic.SetCurrentInteractable(null);

                _parentStateMachine.ChangeState(EState.Idle);
            }
        }

        public InteractLogic(RaftPlayer player, InputLogic inputLogic)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();

            _player = player;
            _inputLogic = inputLogic;

            _uiStateMachine = new();

            IdleState idleState = new IdleState(_uiStateMachine);
            CreateState createState = new CreateState(_uiStateMachine);
            RetargetState retargetState = new RetargetState(_uiStateMachine);
            DestroyState destroyState = new DestroyState(_uiStateMachine);

            idleState.Initialise(this);
            createState.Initialise(this);
            retargetState.Initialise(this);
            destroyState.Initialise(this);

            _uiStateMachine.AddState(EState.Idle, idleState);
            _uiStateMachine.AddState(EState.Create, createState);
            _uiStateMachine.AddState(EState.Retarget, retargetState);
            _uiStateMachine.AddState(EState.Destroy, destroyState);
        }

        public void Tick()
        {
            _targetInteractable = FindTarget();

            _uiStateMachine.Tick();

            AnimateTick();

            InteractTick();
        }

        private IInteractable FindTarget()
        {
            // Detect nearby interactables
            int overlap = Physics.OverlapSphereNonAlloc(_player.transform.position, _player.Data.InteractSettings.Radius, _collidersNonAlloc, _player.Data.InteractSettings.Mask);

            if (overlap == 0)
            {
                return null;
            }

            _nearbyInteractables.Clear();

            // Store any that we are able to interact with
            for (int i = 0; i < overlap; i++)
            {
                Collider collider = _collidersNonAlloc[i];

                float distance = Vector3.Distance(_player.transform.position, collider.transform.position);

                Vector3 direction = (collider.transform.position - _player.transform.position);
                direction.y = 0f;
                direction.Normalize();

                float angle = Vector3.Angle(_player.transform.forward, direction);

                if (distance > _player.Data.InteractSettings.MaxDistance && angle > _player.Data.InteractSettings.MaxAngle)
                {
                    continue;
                }

                if (!collider.TryGetComponent(out IInteractable interactable))
                {
                    continue;
                }

                _nearbyInteractables.Add(new NearbyInteractable(interactable, distance, angle));
            }

            if (_nearbyInteractables.Count == 0)
            {
                return null;
            }

            // Sort by relevance
            _nearbyInteractables.Sort();

            return _nearbyInteractables[0].Interactable;
        }

        private void SetCurrentInteractable(IInteractable interactable)
        {
            _currentInteractable = interactable;
            _animateTimer = 0f;
        }

        private void AnimateTick()
        {
            if (_currentInteractable as Object == null)
            {
                return;
            }

            _animateTimer += Time.deltaTime;

            float amplitude = 0.05f;
            float frequency = 2f;
            float phaseShift = 0f;
            float verticalShift = 0.5f;
            Vector3 offset = Vector3.up * (amplitude * Mathf.Sin(frequency * (_animateTimer - phaseShift)) + verticalShift);

            // Bob up and down slightly above the interactable
            _interactPromptUI.transform.position = _currentInteractable.Position + offset;
        }

        private void InteractTick()
        {
            if (_targetInteractable as Object == null)
            {
                return;
            }

            if (_inputLogic.Interact)
            {
                _targetInteractable.Interact();
            }
        }
    }
}