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
        IStateMachine SubStateMachine { get; }

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

        public IStateMachine SubStateMachine => _subStateMachine;

        protected float _stateTimer;

        public State(StateMachine<TParentStateEnum> parent)
        {
            _parentStateMachine = parent;
        }

        public virtual void Enter()
        {
            _subStateMachine?.Enter();

            _stateTimer = 0f;
        }

        public virtual async Task EnterAsync()
        {
            await (_subStateMachine?.EnterAsync() ?? Task.CompletedTask);
        }

        public virtual void Tick()
        {
            _subStateMachine?.Tick();

            _stateTimer += Time.deltaTime;
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

    public interface IStateMachine : IEnumerable<IState>
    {
        Enum CurrentEnum { get; }
        IState CurrentState { get; }

        event Action<Enum, Enum> OnStateChanged;

        void Tick();
        void ChangeState(int enumValue);
    }

    public class StateMachine<TStateEnum> : IStateMachine, IEnumerable<IState>
        where TStateEnum : Enum
    {
        private Dictionary<TStateEnum, IState> _enumStateMap = new();

        private TStateEnum _currentStateEnum;

        public TStateEnum CurrentStateEnum => _currentStateEnum;
        public Enum CurrentEnum => _currentStateEnum;

        public IState CurrentState => _enumStateMap[_currentStateEnum];

        public event Action<TStateEnum, TStateEnum> OnStateEnumChanged;
        public event Action<Enum, Enum> OnStateChanged;

        public StateMachine()
        {
            // Start off every enum with null. Allows us to skip assigning null to Enum.None
            foreach (TStateEnum stateEnum in Enum.GetValues(typeof(TStateEnum)).Cast<TStateEnum>())
            {
                _enumStateMap.Add(stateEnum, null);
            }
        }

        public IState this[TStateEnum stateEnum]
        {
            get => _enumStateMap[stateEnum];
        }

        public void AddState(TStateEnum stateEnum, IState state)
        {
            _enumStateMap[stateEnum] = state;
        }

        public void ChangeState(int enumValue)
        {
            ChangeState((TStateEnum)(object)enumValue);
        }

        public void ChangeState(TStateEnum stateEnum)
        {
            if (!_enumStateMap.ContainsKey(stateEnum))
            {
                Log.Error("Tried to change to a state that has not been mapped");
                return;
            }

            if (Equals(_currentStateEnum, stateEnum))
            {
                Log.Error("Tried to change to a state we are already in");
                return;
            }

            TStateEnum previous = _currentStateEnum;

            // CurrentState will return the new state once we assign the enum, so we can't just cache the output
            CurrentState?.Exit();
            _ = CurrentState?.ExitAsync();

            _currentStateEnum = stateEnum;

            CurrentState?.Enter();
            _ = CurrentState?.EnterAsync();

            OnStateEnumChanged?.Invoke(previous, _currentStateEnum);
            OnStateChanged?.Invoke(previous, _currentStateEnum);
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

        public IEnumerator<IState> GetEnumerator()
        {
            foreach (IState state in _enumStateMap.Values)
            {
                // It's common to have a .None -> null value
                if (state == null)
                {
                    continue;
                }

                yield return state;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}