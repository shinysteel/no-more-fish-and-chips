using ShinyOwl.Common.Framework;
using UnityEngine;

namespace NoMoreFishAndChips.States
{
    public class StageState : GameplaySubState<ENone>
    {
        private StageStateConfig _config;

        public StageState(StateMachine<EGameplayState> parent) : base(parent)
        {
        }

        public override void InitialiseConfig(GameplayStateConfig config)
        {
            _config = config.StageStateConfig;
        }

        public override void Enter()
        {
            _context.WaveSpawner.SetStageData(_config.DefaultStageData);
        }
    }
}