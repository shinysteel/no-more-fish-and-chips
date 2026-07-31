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
            if (_voyage?.IsComplete ?? false)
            {
                _voyage.OnWaveIndexChanged -= HandleVoyageWaveIndexChanged;
                _voyage.OnStageChanged -= HandleVoyageStageChanged;
                _voyage.OnStageComplete -= HandleVoyageStageComplete;
                _voyage = null;
            }

            if (_voyage == null)
            {
                _voyage = new Voyage(_temperateSeaVoyageData);
                _voyage.OnWaveIndexChanged += HandleVoyageWaveIndexChanged;
                _voyage.OnStageChanged += HandleVoyageStageChanged;
                _voyage.OnStageComplete += HandleVoyageStageComplete;
            }

            _voyage.Continue();
        }

        private void HandleVoyageWaveIndexChanged(int index)
        {
            _netWaveIndex.value = index;
        }

        private void HandleVoyageStageChanged(IStage stage)
        {
            OnStageChanged?.Invoke(stage);
        }

        private void HandleVoyageStageComplete()
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