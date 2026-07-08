using ShinyOwl.Common;
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

        public override void InitialiseContext(GameplayContext context)
        {
            base.InitialiseContext(context);

            _context.WaveSpawner.OnStageComplete += HandleStageComplete;
        }

        ~StageState()
        {
            if (_context.WaveSpawner != null)
            {
                _context.WaveSpawner.OnStageComplete -= HandleStageComplete;
            }
        }

        public override void Enter()
        {
            _context.WaveSpawner.SetStageData(_config.ClamClusterStageData);
        }

        private void HandleStageComplete()
        {
            Log.Info("change back to lobby state");
        }
    }
}