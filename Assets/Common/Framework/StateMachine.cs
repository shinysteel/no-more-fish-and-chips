using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ShinyOwl.Common.Framework
{
    /// <summary>
    /// Goals:
    /// 1. We achieve a HFSM where states can implement sub state machines
    /// 2. States are aware of their 'parent', and can request to change to their related sub states
    /// 3. The state machine is generic and does not require extra top-level 'state' classes
    /// 4. The systems needs to be safe and not allow state machines change to states outside their scope
    /// </summary>

    public interface IState
    {
        void Enter();
        Task EnterAsync();
        void Update();
        void Exit();
        Task ExitAsync();
    }

    public class State<TParentStateEnum, TSubStateEnum> : IState
        where TParentStateEnum : Enum
        where TSubStateEnum    : Enum
    {
        protected StateMachine<TParentStateEnum> _parentStateMachine;
        protected StateMachine<TSubStateEnum> _subStateMachine;

        public State(StateMachine<TParentStateEnum> parent)
        {
            _parentStateMachine = parent;
        }

        public virtual void Enter()
        {
            _subStateMachine?.Enter();
        }

        public virtual async Task EnterAsync()
        {
            await (_subStateMachine?.EnterAsync() ?? Task.CompletedTask);
        }

        public virtual void Update()
        {
            _subStateMachine?.Update();
        }

        public virtual void Exit()
        {
            _subStateMachine?.Exit();
        }

        public virtual async Task ExitAsync()
        {
            await (_subStateMachine?.ExitAsync() ?? Task.CompletedTask);
        }

        protected void ChangeState(TSubStateEnum stateEnum)
        {
            _subStateMachine?.ChangeState(stateEnum);   
        }
    }

    public class StateMachine<TStateEnum> 
        where TStateEnum : Enum
    {
        private Dictionary<TStateEnum, IState> _enumStateMap = new();
        private IState _currentState;

        public IState CurrentState => _currentState;

        public StateMachine()
        {
            // Start off every enum with null. Allows us to skip assigning null to Enum.None
            foreach (TStateEnum stateEnum in Enum.GetValues(typeof(TStateEnum)).Cast<TStateEnum>())
            {
                _enumStateMap.Add(stateEnum, null);
            }
        }

        public void AddState(TStateEnum stateEnum, IState state)
        {
            _enumStateMap[stateEnum] = state;
        }

        public void ChangeState(TStateEnum stateEnum)
        {
            if (!_enumStateMap.TryGetValue(stateEnum, out IState state))
            {
                Debugger.LogError(this, "Tried to change to a state that has not been mapped");
                return;
            }

            if (_currentState == state)
            {
                Debugger.LogError(this, "Tried to change to a state we are already in");
                return;
            }

            _currentState?.Exit();
            _ = _currentState?.ExitAsync();
            _currentState = state;
            _currentState?.Enter();
            _ = _currentState?.EnterAsync();
        }

        public void Enter()
        {
            _currentState?.Enter();
        }

        public async Task EnterAsync()
        {
            await (_currentState?.EnterAsync() ?? Task.CompletedTask);
        }

        public void Update()
        {
            _currentState?.Update();
        }

        public void Exit()
        {
            _currentState?.Exit();
        }

        public async Task ExitAsync()
        {
            await (_currentState?.ExitAsync() ?? Task.CompletedTask);
        }
    }
}