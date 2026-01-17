using ShinyOwl.Common.Framework;
using System;
using UnityEngine;

namespace FishFlingers.States
{
    public interface IMainState
    {
        void Initialise(StateManagerConfig config);
    }

    public abstract class MainState<TParentStateEnum, TSubStateEnum> : State<TParentStateEnum, TSubStateEnum>, IMainState
        where TParentStateEnum : Enum
        where TSubStateEnum : Enum
    {
        public MainState(StateMachine<TParentStateEnum> parent) : base(parent) 
        { }

        public virtual void Initialise(StateManagerConfig config)
        { }
    }
}