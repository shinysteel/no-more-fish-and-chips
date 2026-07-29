using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

using EntityId = NoMoreFishAndChips.Entities.EntityId;
using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;

namespace NoMoreFishAndChips.States
{
    [Serializable]
    public class WaveStep
    {
        [SerializeField] private EntityId _entityId;
        [SerializeField] private int _count;
        [SerializeField] private float _interval;

        public EntityId EntityId => _entityId;
        public int Count => _count;
        public float Interval => _interval;
    }

    [Serializable]
    public class Wave
    {
        [SerializeField] private WaveStep[] _steps;

        public WaveStep[] Steps => _steps;
    }

    public class Stage : IEntityManagerListener
    {
        private EntityManager _entityManager;

        private StageData _data;
        private SyncVar<int> _netWaveIndex;

        private WaveStep _waveStep;
        private int _waveStepIndex;
        private bool _areWavesDefeated;

        private StateMachine<EState> _stateMachine;

        private bool IsLastWaveStep => _waveStepIndex >= _data.Waves[_netWaveIndex.value].Steps.Length - 1;
        private bool IsLastWave => _netWaveIndex.value >= _data.Waves.Length - 1;
        public bool IsComplete => IsLastWave && _areWavesDefeated;

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

        public Stage(StageData data, SyncVar<int> netWaveIndex)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();

            _entityManager.AddListener(this);

            _data = data;
            _netWaveIndex = netWaveIndex;

            _stateMachine = new();

            SpawnState spawnState = new SpawnState(_stateMachine, this);
            WaitState waitState = new WaitState(_stateMachine, this);

            _stateMachine.AddState(EState.Spawn, spawnState);
            _stateMachine.AddState(EState.Wait, waitState);

            NextWave();
            _stateMachine.ChangeState(EState.Spawn);
        }

        ~Stage()
        {
            _entityManager?.RemoveListener(this);
        }

        public void Tick()
        {
            _stateMachine.Tick();
        }

        public void NextWaveStep()
        {
            _waveStepIndex++;
            _waveStep = _data.Waves[_netWaveIndex.value].Steps[_waveStepIndex];
        }

        public void NextWave()
        {
            if (_waveStep != null)
            {
                _netWaveIndex.value++;
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

    public class Voyage
    {
        private VoyageData _data;
        private SyncVar<int> _netWaveIndex;

        private Stage _stage;
        private int _stageIndex;

        public bool IsComplete => _stageIndex >= _data.StageDatas.Length - 1 && _stage.IsComplete;

        public Stage Stage => _stage;

        public event Action OnStageComplete;

        public Voyage(VoyageData data, SyncVar<int> netWaveIndex)
        {
            _data = data;
            _netWaveIndex = netWaveIndex;
        }

        public void Continue()
        {
            if (_stage != null)
            {
                _stage.OnWaveComplete -= HandleWaveComplete;
                _stage = null;
                _stageIndex++;
            }

            if (_stageIndex >= _data.StageDatas.Length)
            {
                return;
            }

            _stage = new Stage(_data.StageDatas[_stageIndex], _netWaveIndex);
            _stage.OnWaveComplete += HandleWaveComplete;
        }

        public void Tick()
        {
            _stage.Tick();
        }

        private void HandleWaveComplete()
        {
            if (_stage.IsComplete)
            {
                OnStageComplete?.Invoke();
            }
        }
    }

    public class VoyageRunner : GameplayBehaviour
    {
        [SerializeField] private VoyageData _temperateSeaVoyageData;

        private Voyage _voyage;
        private SyncVar<int> _netWaveIndex = new SyncVar<int>(ownerAuth: true);

        public int WaveIndex => _netWaveIndex.value;

        public event Action<int> OnWaveIndexChanged;
        public event Action OnStageComplete;

        protected override void OnSpawned()
        {
            base.OnSpawned();

            _netWaveIndex.onChanged += HandleNetWaveIndexChanged;
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _netWaveIndex.onChanged -= HandleNetWaveIndexChanged;
        }

        private void HandleNetWaveIndexChanged(int index)
        {
            OnWaveIndexChanged?.Invoke(index);
        }

        public void ContinueVoyage()
        {
            if (_voyage?.IsComplete != false)
            {
                _voyage = new Voyage(_temperateSeaVoyageData, _netWaveIndex);
                _voyage.OnStageComplete += HandleStageComplete;
            }

            _voyage.Continue();
        }

        private void HandleStageComplete()
        {
            OnStageComplete?.Invoke();
        }

        private void Update()
        {
            if (!isOwner)
            {
                return;
            }

            _voyage?.Tick();
        }
    }
}