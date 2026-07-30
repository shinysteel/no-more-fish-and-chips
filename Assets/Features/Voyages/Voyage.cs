using NoMoreFishAndChips.States;
using PurrNet;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Voyages
{
    public interface IVoyage
    {
        IStage Stage { get; }
    }

    public class Voyage : IVoyage
    {
        private VoyageData _data;
        private SyncVar<int> _netWaveIndex;

        private Stage _stage;
        private int _stageIndex;

        public bool IsComplete => _stageIndex >= _data.StageDatas.Length - 1 && _stage.IsComplete;

        IStage IVoyage.Stage => _stage;

        public event Action<IStage> OnStageChanged;
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

            OnStageChanged?.Invoke(_stage);
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
            _stage.Tick();
        }
    }
}