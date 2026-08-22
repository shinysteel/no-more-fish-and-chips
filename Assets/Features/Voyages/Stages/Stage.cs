using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.States;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common.Framework;
using UnityEngine;
using System;
using System.Linq;

using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;
using ShinyOwl.Common;
using NoMoreFishAndChips.Hitboxes;
using System.Collections.Generic;

namespace NoMoreFishAndChips.Voyages
{
    public class Stage : IEntityManagerListener, IHitboxManagerListener
    {
        private EntityManager _entityManager;
        private HitboxManager _hitboxManager;
        private NetworkManager _networkManager;

        private GameplayContext _context;
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
                    for (int i = 0; i < Mathf.CeilToInt(_stage._networkManager.PurrnetPlayers.Count / 2f); i++)
                    {
                        _stage._entityManager.Spawn(_stage._waveStep.EntityId, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition });
                    }

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
            private float _minDuration = 3f;
            private float _maxDuration = 5f;

            public WaitState(StateMachine<EState> parent, Stage stage) : base(parent, stage)
            { }

            public override void Enter()
            {
                base.Enter();

                if (_stage.IsLastWaveStep && _stage.IsLastWave)
                {
                    Tween.Delay(_minDuration, _stage.RefreshAreWavesDefeated);
                }
            }

            public override void Tick()
            {
                base.Tick();
                
                if (_stateTimer < _minDuration)
                {
                    return;
                }

                if (_stateTimer < _maxDuration && _stage.IsLastWaveStep && !_stage._areWavesDefeated)
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

        public Stage(GameplayContext context, StageData data)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _entityManager.AddListener(this);
            _hitboxManager.AddListener(this);

            _context = context;
            _data = data;

            foreach (RaftTile tile in _context.Raft.Tiles.Values)
            {
                HandleTileChanged(tile.Cell, null, tile);
            }

            _context.Raft.OnTileChanged += HandleTileChanged;

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
            _entityManager.RemoveListener(this);
            _hitboxManager.RemoveListener(this);

            if (_context.Raft != null)
            {
                _context.Raft.OnTileChanged -= HandleTileChanged;
            }

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

            _areWavesDefeated = false;
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

        void IHitboxManagerListener.OnHitboxesChanged(IReadOnlyList<Hitbox> hitboxes)
        {
            RefreshAreWavesDefeated();
        }

        private void HandleTileChanged(Vector2Int cell, RaftTile previous, RaftTile current)
        {
            if (previous != null)
            {
                previous.EntityDefeatLogic.OnIsDefeatedChanged -= HandleTileIsDefeatedChanged;
            }

            if (current != null)
            {
                current.EntityDefeatLogic.OnIsDefeatedChanged += HandleTileIsDefeatedChanged;
            }
        }

        private void HandleTileIsDefeatedChanged(bool defeated)
        {
            RefreshAreWavesDefeated();
        }

        private void RefreshAreWavesDefeated()
        {   
            if (_entityManager.Entities.Any(entity => entity is Character character && character.EntityDefinitionData.Alliance == EntityAlliance.Enemy))
            {
                return;
            }

            if (_hitboxManager.Hitboxes.Any(hitbox => hitbox.Data.Alliance == EntityAlliance.Enemy || hitbox.Data.Alliance == EntityAlliance.Neutral))
            {
                return;
            }

            if (_context.Raft.Tiles.Values.Any(tile => tile.EntityDefeatLogic.IsDefeated))
            {
                return;
            }

            _areWavesDefeated = true;
        }
    }
}