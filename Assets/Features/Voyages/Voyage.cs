using NoMoreFishAndChips.States;
using PurrNet;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Voyages
{
    public class Voyage
    {
        private GameplayContext _context;
        private VoyageData _data;

        private Stage _stage;
        private int _stageIndex;

        public int WaveIndex => _stage.WaveIndex;
        public bool IsComplete => _stageIndex >= _data.StageDatas.Length - 1 && _stage.IsComplete;

        public event Action<int> OnStageIndexChanged;
        public event Action<int> OnWaveIndexChanged;
        public event Action OnStageComplete;

        public Voyage(GameplayContext context, VoyageData data)
        {
            _context = context;
            _data = data;
        }

        public void Dispose()
        {
            _stage?.Dispose();
        }

        public void Continue()
        {
            if (_stage != null)
            {
                _stage.OnWaveIndexChanged -= HandleWaveIndexChanged;
                _stage.OnWaveComplete -= HandleWaveComplete;
                _stage = null;
                _stageIndex++;
                OnStageIndexChanged?.Invoke(_stageIndex);
            }

            if (_stageIndex >= _data.StageDatas.Length)
            {
                return;
            }

            _stage = new Stage(_context, _data.StageDatas[_stageIndex]);
            _stage.OnWaveIndexChanged += HandleWaveIndexChanged;
            _stage.OnWaveComplete += HandleWaveComplete;
        }

        private void HandleWaveIndexChanged(int index)
        {
            OnWaveIndexChanged?.Invoke(index);
        }

        private void HandleWaveComplete()
        {
            if (_stage.IsComplete)
            {
                OnStageComplete?.Invoke();
            }
        }

        public void Tick()
        {
            _stage?.Tick();
        }
    }
}