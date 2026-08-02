using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.States;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common.Framework;
using UnityEngine;
using System;
using System.Linq;

using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;

namespace NoMoreFishAndChips.Voyages
{
    public class Stage : IEntityManagerListener
    {
        private EntityManager _entityManager;

        private StageData _data;

        private int _waveIndex;

        private WaveStep _waveStep;
        private int _waveStepIndex;
        private bool _areWavesDefeated;

        private StateMachine<EState> _stateMachine;

        public int WaveIndex => _waveIndex;
        private bool IsLastWaveStep => _waveStepIndex >= _data.Waves[_waveIndex].Steps.Length - 1;
        private bool IsLastWave => _waveIndex >= _data.Waves.Length - 1;
        public bool IsComplete => IsLastWave && _areWavesDefeated;

        public event Action<int> OnWaveIndexChanged;
        public event Action OnWaveComplete;

        private enum EState
        {
            None,
            Spawn,
            Wait
        }

        private abstract class State : State<EState, ENone>
        {
            protected Stage _stage;

            protected State(StateMachine<EState> parent, Stage stage) : base(parent)
            {
                _stage = stage;
            }
        }

        private class SpawnState : State
        {
            private float _stepTimer;
            private int _spawnCounter;

            public SpawnState(StateMachine<EState> parent, Stage stage) : base(parent, stage)
            { }

            public override void Enter()
            {
                base.Enter();

                _stepTimer = 0f;
                _spawnCounter = 0;
            }

            public override void Tick()
            {
                base.Tick();

                _stepTimer += Time.deltaTime;

                while (_spawnCounter < _stage._waveStep.Count && _stepTimer >= _stage._waveStep.Interval)
                {
                    _stage._entityManager.Spawn(_stage._waveStep.EntityId, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition });
                    _spawnCounter++;
                    _stage._areWavesDefeated = false;
                    _stepTimer -= _stage._waveStep.Interval;
                }

                if (_spawnCounter == _stage._waveStep.Count)
                {
                    _parentStateMachine.ChangeState(EState.Wait);
                }
            }
        }

        private class WaitState : State
        {
            private float _duration = 3f;

            public WaitState(StateMachine<EState> parent, Stage stage) : base(parent, stage)
            { }

            public override void Enter()
            {
                base.Enter();

                if (_stage.IsLastWaveStep && _stage.IsLastWave)
                {
                    Tween.Delay(_duration, _stage.RefreshAreWavesDefeated);
                }
            }

            public override void Tick()
            {
                base.Tick();

                if (_stateTimer < _duration)
                {
                    return;
                }

                if (!_stage.IsLastWaveStep)
                {
                    _stage.NextWaveStep();
                    _parentStateMachine.ChangeState(EState.Spawn);
                }
                else if (!_stage.IsLastWave)
                {
                    _stage.NextWave();
                    _parentStateMachine.ChangeState(EState.Spawn);
                }
                else if (_stage._areWavesDefeated)
                {
                    _parentStateMachine.ChangeState(EState.None);
                }
            }

            public override void Exit()
            {
                base.Exit();

                _stage.OnWaveComplete?.Invoke();
            }
        }

        public Stage(StageData data)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();

            _entityManager.AddListener(this);

            _data = data;

            _stateMachine = new();

            SpawnState spawnState = new SpawnState(_stateMachine, this);
            WaitState waitState = new WaitState(_stateMachine, this);

            _stateMachine.AddState(EState.Spawn, spawnState);
            _stateMachine.AddState(EState.Wait, waitState);

            NextWave();
            _stateMachine.ChangeState(EState.Spawn);
        }

        public void Dispose()
        {
            _entityManager?.RemoveListener(this);

            _stateMachine.Dispose();
        }

        public void Tick()
        {
            _stateMachine.Tick();
        }

        public void NextWaveStep()
        {
            _waveStepIndex++;
            _waveStep = _data.Waves[_waveIndex].Steps[_waveStepIndex];
        }

        public void NextWave()
        {
            if (_waveStep != null)
            {
                _waveIndex++;
                OnWaveIndexChanged?.Invoke(_waveIndex);
            }

            _waveStepIndex = -1;
            NextWaveStep();
        }

        void IEntityManagerListener.OnEntityDespawned(Entity entity)
        {
            RefreshAreWavesDefeated();
        }

        private void RefreshAreWavesDefeated()
        {
            if (_entityManager.Entities.Any(entity => entity is Character character && character.EntityDefinitionData.Alliance == EntityAlliance.Enemy))
            {
                return;
            }

            // This condition won't work unless we can listen to an event for this interaction
            //if (_entityManager.Entities.Any(entity => entity is RaftTile tile && tile.TileDefeatModule.IsSinking))
            //{
            //    return;
            //}

            _areWavesDefeated = true;
        }
    }
}