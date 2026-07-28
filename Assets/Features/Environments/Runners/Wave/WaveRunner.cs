using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using PurrNet;
using ShinyOwl.Common;
using System;
using System.Linq;
using UnityEngine;

using EntityId = NoMoreFishAndChips.Entities.EntityId;
using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;
using Random = UnityEngine.Random;

namespace NoMoreFishAndChips.Environments
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

    public class WaveRunner : GameplayBehaviour, IEntityManagerListener
    {
        [SerializeField] private float _stepEndDelay = 3f;
        
        private StageData _stageData;

        private SyncVar<int> _netWaveIndex = new SyncVar<int>(ownerAuth: true);
        public int WaveIndex => _netWaveIndex.value;

        private int _stepIndex;
        private int _spawnCounter;
        private float _stepTimer;

        private bool _isWaveDefeated;

        public event Action<int> OnWaveIndexChanged;
        public event Action OnStageComplete;

        protected override void OnSpawned()
        {
            _entityManager.AddListener(this);

            _netWaveIndex.onChanged += HandleNetWaveIndexChanged;

            base.OnSpawned();
        }

        protected override void OnDespawned()
        {
            _entityManager?.RemoveListener(this);

            _netWaveIndex.onChanged -= HandleNetWaveIndexChanged;

            base.OnDespawned();
        }
        
        private void HandleNetWaveIndexChanged(int index)
        {
            OnWaveIndexChanged?.Invoke(index);
        }

        public void SetStageData(StageData data)
        {
            _netWaveIndex.value = 0;
            _stepIndex = 0;
            _spawnCounter = 0;
            _stepTimer = 0f;
            _stageData = data;

            // Assume the wave is defeated until an enemy has actually spawned
            _isWaveDefeated = true;
        }

        private void Update()
        {
            if (!isOwner)
            {
                return;
            }
            
            if (_stageData == null)
            {
                return;
            }

            WaveUpdate();
            CompleteUpdate();
        }

        private void WaveUpdate()
        {
            _stepTimer += Time.deltaTime;

            while (true)
            {
                Wave wave = _stageData.Waves[_netWaveIndex.value];
                WaveStep step = wave.Steps[_stepIndex];

                if (!SpawnUpdate(step))
                {
                    break;
                }

                if (!DelayUpdate(step))
                {
                    break;
                }

                NextStep();

                if (_stepIndex == wave.Steps.Length)
                {
                    NextWave();
                }

                if (_netWaveIndex.value == _stageData.Waves.Length)
                {
                    break;
                }
            }
        }

        private bool SpawnUpdate(WaveStep step)
        {
            if (_spawnCounter == step.Count)
            {
                return true;
            }

            while (_spawnCounter < step.Count && _stepTimer >= step.Interval)
            {
                _isWaveDefeated = false;
                _entityManager.Spawn(step.EntityId, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition });

                _spawnCounter++;
                _stepTimer -= step.Interval;
            }
            
            return _spawnCounter == step.Count;
        }

        private bool DelayUpdate(WaveStep step)
        {
            if (_stepTimer < _stepEndDelay)
            {
                return false;
            }

            if (!_isWaveDefeated)
            {
                _stepTimer = Mathf.Min(_stepTimer, _stepEndDelay);
                return false;
            }

            _stepTimer -= _stepEndDelay;
            return true;
        }

        private void NextStep()
        {
            _stepIndex++;
            _spawnCounter = 0;
        }

        private void NextWave()
        {
            _netWaveIndex.value++;
            _stepIndex = 0;
        }

        void IEntityManagerListener.OnEntityDespawned(Entity entity)
        {
            if (!isOwner)
            {
                return;
            }

            RefreshIsWaveComplete();
        }

        private void RefreshIsWaveComplete()
        {
            if (_stageData == null)
            {
                return;
            }

            if (_spawnCounter < _stageData.Waves[_netWaveIndex.value].Steps[_stepIndex].Count)
            {
                return;
            }

            if (_entityManager.Entities.Any(entity => entity is Character character && character.EntityDefinitionData.Alliance == EntityAlliance.Enemy))
            {
                return;
            }

            if (_entityManager.Entities.Any(entity => entity is RaftTile tile && tile.TileDefeatModule.IsSinking))
            {
                return;
            }

            _isWaveDefeated = true;
        }

        private void CompleteUpdate()
        {
            if (_netWaveIndex.value < _stageData.Waves.Length)
            {
                return;
            }

            OnStageComplete?.Invoke();

            _stageData = null;
        }
    }
}