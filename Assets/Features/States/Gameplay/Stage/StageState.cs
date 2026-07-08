using ShinyOwl.Common.Framework;
using UnityEngine;

namespace NoMoreFishAndChips.States
{
    public class StageState : GameplaySubState
    {
        private StageStateConfig _config;

        public StageState(StateMachine<EGameplayState> parent) : base(parent)
        {
        }

        public override void Initialise(StateManagerConfig config)
        {
            _config = config.GameplayStateConfig.StageStateConfig;
        }

        public override void Enter()
        {
            _context.WaveSpawner.SetStageData(_config.DefaultStageData);
        }
    }
}