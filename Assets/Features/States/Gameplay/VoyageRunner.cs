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
using NoMoreFishAndChips.Voyages;

namespace NoMoreFishAndChips.States
{
    public class VoyageRunner : GameplayBehaviour
    {
        [SerializeField] private VoyageData _temperateSeaVoyageData;

        private Voyage _voyage;
        private SyncVar<int> _netWaveIndex = new SyncVar<int>(ownerAuth: true);

        public int WaveIndex => _netWaveIndex.value;

        public IVoyage Voyage => _voyage;

        public event Action<int> OnWaveIndexChanged;
        public event Action<IStage> OnStageChanged;
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
                _voyage.OnStageChanged += HandleStageChanged;
                _voyage.OnStageComplete += HandleStageComplete;
            }

            _voyage.Continue();
        }

        private void HandleStageChanged(IStage stage)
        {
            OnStageChanged?.Invoke(stage);
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