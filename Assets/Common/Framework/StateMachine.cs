using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ShinyOwl.Common.Framework
{
    // When wanting to declare a state that has no SubState, you can use this enum type
    public enum ENone { None }

    public interface IState
    {
        void Enter();
        Task EnterAsync();
        void Tick();
        void Exit();
        Task ExitAsync();
    }

    public abstract class State<TParentStateEnum, TSubStateEnum> : IState
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

        public virtual void Tick()
        {
            _subStateMachine?.Tick();
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

        private TStateEnum _currentEnum;

        public TStateEnum CurrentEnum => _currentEnum;

        public StateMachine()
        {
            // Start off every enum with null. Allows us to skip assigning null to Enum.None
            foreach (TStateEnum stateEnum in Enum.GetValues(typeof(TStateEnum)).Cast<TStateEnum>())
            {
                _enumStateMap.Add(stateEnum, null);
            }
        }

        private IState GetCurrentState()
        {
            return _enumStateMap[_currentEnum];
        }

        public void AddState(TStateEnum stateEnum, IState state)
        {
            _enumStateMap[stateEnum] = state;
        }

        public void ChangeState(TStateEnum stateEnum)
        {
            if (!_enumStateMap.ContainsKey(stateEnum))
            {
                Debugger.LogError(this, "Tried to change to a state that has not been mapped");
                return;
            }

            if (Equals(_currentEnum, stateEnum))
            {
                Debugger.LogError(this, "Tried to change to a state we are already in");
                return;
            }

            // GetCurrentState will return the new state once we assign the enum, so we can't just cache the output
            GetCurrentState()?.Exit();
            _ = GetCurrentState()?.ExitAsync();

            _currentEnum = stateEnum;

            GetCurrentState()?.Enter();
            _ = GetCurrentState()?.EnterAsync();
        }

        public void Enter()
        {
            GetCurrentState()?.Enter();
        }

        public async Task EnterAsync()
        {
            await (GetCurrentState()?.EnterAsync() ?? Task.CompletedTask);
        }

        public void Tick()
        {
            GetCurrentState()?.Tick();
        }

        public void Exit()
        {
            GetCurrentState()?.Exit();
        }

        public async Task ExitAsync()
        {
            await (GetCurrentState()?.ExitAsync() ?? Task.CompletedTask);
        }
    }
}