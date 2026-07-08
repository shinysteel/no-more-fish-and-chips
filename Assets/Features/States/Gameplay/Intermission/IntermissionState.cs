using ShinyOwl.Common.Framework;
using UnityEngine;

namespace NoMoreFishAndChips.States
{
    public class IntermissionState : GameplaySubState<ENone>
    {
        private IntermissionStateConfig _config;

        public IntermissionState(StateMachine<EGameplayState> parent) : base(parent)
        {
        }

        public override void InitialiseConfig(GameplayStateConfig config)
        {
            _config = config.IntermissionStateConfig;
        }
    }
}