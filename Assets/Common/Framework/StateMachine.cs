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
        void FixedTick();
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

        public virtual void FixedTick()
        {
            _subStateMachine?.FixedTick();
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

        public IState CurrentState => _enumStateMap[_currentEnum];

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
            if (!_enumStateMap.ContainsKey(stateEnum))
            {
                Log.Error("Tried to change to a state that has not been mapped");
                return;
            }

            if (Equals(_currentEnum, stateEnum))
            {
                Log.Error("Tried to change to a state we are already in");
                return;
            }

            // CurrentState will return the new state once we assign the enum, so we can't just cache the output
            CurrentState?.Exit();
            _ = CurrentState?.ExitAsync();

            _currentEnum = stateEnum;

            CurrentState?.Enter();
            _ = CurrentState?.EnterAsync();
        }

        public void Enter()
        {
            CurrentState?.Enter();
        }

        public async Task EnterAsync()
        {
            await (CurrentState?.EnterAsync() ?? Task.CompletedTask);
        }

        public void Tick()
        {
            CurrentState?.Tick();
        }

        public void FixedTick()
        {
            CurrentState?.FixedTick();
        }

        public void Exit()
        {
            CurrentState?.Exit();
        }

        public async Task ExitAsync()
        {
            await (CurrentState?.ExitAsync() ?? Task.CompletedTask);
        }
    }
}