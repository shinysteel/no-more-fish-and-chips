using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
using PurrNet;
using ShinyOwl.Common;
using System;
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
        [SerializeField] private float _endDelay;

        public EntityId EntityId => _entityId;
        public int Count => _count;
        public float Interval => _interval;
        public float EndDelay => _endDelay;
    }

    [Serializable]
    public class Wave
    {
        [SerializeField] private WaveStep[] _steps;

        public WaveStep[] Steps => _steps;
    }

    public class WaveRunner : GameplayBehaviour
    {
        private StageData _stageData;

        private SyncVar<int> _netWaveIndex = new SyncVar<int>(ownerAuth: true);
        private int _waveIndex;
        public int WaveIndex => _waveIndex;

        private int _stepIndex;
        private int _spawnCounter;
        private float _stepTimer;

        public event Action<int> OnWaveIndexChanged;
        public event Action OnStageComplete;

        protected override void OnSpawned()
        {
            _netWaveIndex.onChanged += HandleNetWaveIndexChanged;

            base.OnSpawned();
        }

        protected override void OnDespawned()
        {
            _netWaveIndex.onChanged -= HandleNetWaveIndexChanged;

            base.OnDespawned();
        }
        
        private void HandleNetWaveIndexChanged(int netWaveIndex)
        {
            _waveIndex = netWaveIndex;
            OnWaveIndexChanged?.Invoke(_waveIndex);
        }

        public void SetStageData(StageData data)
        {
            _netWaveIndex.value = 0;
            _stepIndex = 0;
            _spawnCounter = 0;
            _stepTimer = 0f;

            _stageData = data;
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
            if (_netWaveIndex.value == _stageData.Waves.Length)
            {
                return;
            }

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
                _entityManager.Spawn(step.EntityId, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition });
                _spawnCounter++;
                _stepTimer -= step.Interval;
            }
            
            return _spawnCounter == step.Count;
        }

        private bool DelayUpdate(WaveStep step)
        {
            if (_stepTimer >= step.EndDelay)
            {
                _stepTimer -= step.EndDelay;
                return true;
            }
            else
            {
                return false;
            }
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