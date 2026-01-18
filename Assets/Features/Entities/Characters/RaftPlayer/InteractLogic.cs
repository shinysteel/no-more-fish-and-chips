using NUnit.Framework;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;
using System.Collections.Generic;
using FishFlingers.UI;
using ShinyOwl.Common.Framework;

namespace FishFlingers.Entities
{
    public class InteractLogic
    {
        private UIManager _uiManager;

        private RaftPlayer _player;

        private StateMachine<EState> _stateMachine;

        private InteractPromptUI _interactPromptUI;
        private float _animateTimer;

        private IInteractable _currentInteractable;
        private IInteractable _targetInteractable;

        private Collider[] _collidersNonAlloc = new Collider[MaxOverlaps];
        private List<IInteractable> _interactables = new List<IInteractable>(MaxOverlaps);

        private const int MaxOverlaps = 10;

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

        public InteractLogic(RaftPlayer player)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();

            _player = player;

            _stateMachine = new();

            IdleState idleState = new IdleState(_stateMachine);
            CreateState createState = new CreateState(_stateMachine);
            RetargetState retargetState = new RetargetState(_stateMachine);
            DestroyState destroyState = new DestroyState(_stateMachine);

            idleState.Initialise(this);
            createState.Initialise(this);
            retargetState.Initialise(this);
            destroyState.Initialise(this);

            _stateMachine.AddState(EState.Idle, idleState);
            _stateMachine.AddState(EState.Create, createState);
            _stateMachine.AddState(EState.Retarget, retargetState);
            _stateMachine.AddState(EState.Destroy, destroyState);
        }

        public void Tick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            _targetInteractable = FindTarget();

            _stateMachine.Tick();

            AnimateTick();
        }

        private IInteractable FindTarget()
        {
            // Detect nearby interactables
            int overlap = Physics.OverlapSphereNonAlloc(_player.transform.position, _player.Data.InteractSettings.Radius, _collidersNonAlloc, _player.Data.InteractSettings.Mask);

            if (overlap == 0)
            {
                return null;
            }

            _interactables.Clear();

            // Store any that we are able to interact with
            for (int i = 0; i < overlap; i++)
            {
                Collider collider = _collidersNonAlloc[i];

                Vector3 direction = (collider.transform.position - _player.transform.position);
                direction.y = 0f;
                direction.Normalize();

                if (Vector3.Angle(_player.transform.forward, direction) > _player.Data.InteractSettings.MaxAngle)
                {
                    continue;
                }

                if (!collider.TryGetComponent(out IInteractable interactable))
                {
                    continue;
                }

                _interactables.Add(interactable);
            }

            if (_interactables.Count == 0)
            {
                return null;
            }

            // Calculate the closest
            IInteractable closest = null;
            float minDist = Mathf.Infinity;

            foreach (IInteractable interactable in _interactables)
            {
                float distance = Vector3.Distance(_player.transform.position, interactable.Position);

                if (distance >= minDist)
                {
                    continue;
                }

                closest = interactable;
                minDist = distance;
            }

            return closest;
        }

        private void SetCurrentInteractable(IInteractable interactable)
        {
            _currentInteractable = interactable;
            _animateTimer = 0f;
        }

        private void AnimateTick()
        {
            if (_currentInteractable == null)
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
    }
}