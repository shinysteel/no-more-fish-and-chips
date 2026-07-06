using ShinyOwl.Common.Framework;
using UnityEngine;

namespace NoMoreFishAndChips.States
{
    public class IntermissionState : GameplaySubState
    {
        private IntermissionStateConfig _config;

        public IntermissionState(StateMachine<EGameplayState> parent) : base(parent)
        {
        }

        public override void Initialise(StateManagerConfig config)
        {
            _config = config.GameplayStateConfig.IntermissionStateConfig;
        }
    }
}