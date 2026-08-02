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
using System.Collections.Generic;

namespace NoMoreFishAndChips.States
{
    public enum VoyageResult
    {
        None,
        Victory,
        Defeat
    }

    public class VoyageRunner : GameplayBehaviour
    {
        [SerializeField] private VoyageData _temperateSeaVoyageData;

        // Voyage only exists for the host
        private Voyage _voyage;

        private StageData _stageData;

        private SyncVar<VoyageResult> _netVoyageResult = new SyncVar<VoyageResult>(ownerAuth: true);
        private SyncList<StageId> _netStageIds = new SyncList<StageId>(ownerAuth: true);
        private SyncVar<int> _netStageIndex = new SyncVar<int>(ownerAuth: true);
        private SyncVar<int> _netWaveIndex = new SyncVar<int>(ownerAuth: true);

        public StageData StageData => _stageData;
        public VoyageResult VoyageResult => _netVoyageResult.value;
        public int WaveIndex => _netWaveIndex.value;

        public event Action<StageData> OnStageDataChanged;
        public event Action<int> OnWaveIndexChanged;
        public event Action OnStageComplete;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _voyage?.Dispose();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            _netStageIds.onChanged += HandleNetStageIdsChanged;
            _netStageIndex.onChanged += HandleNetStageIndexChanged;
            _netWaveIndex.onChanged += HandleNetWaveIndexChanged;
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _netStageIds.onChanged -= HandleNetStageIdsChanged;
            _netStageIndex.onChanged -= HandleNetStageIndexChanged;
            _netWaveIndex.onChanged -= HandleNetWaveIndexChanged;
        }

        private void HandleNetStageIdsChanged(SyncListChange<StageId> change)
        {
            RefreshStageData();
        }

        private void HandleNetStageIndexChanged(int index)
        {
            RefreshStageData();
        }

        private void HandleNetWaveIndexChanged(int index)
        {
            OnWaveIndexChanged?.Invoke(index);
        }

        private void RefreshStageData()
        {
            StageData previous = _stageData;

            _stageData = _netStageIndex.value < _netStageIds.Count ? _voyageManager.GetStageData(_netStageIds[_netStageIndex.value]) : null;

            if (_stageData != previous)
            {
                OnStageDataChanged?.Invoke(_stageData);
            }
        }

        public void ContinueVoyage()
        {
            if (_voyage?.IsComplete ?? false)
            {
                _voyage.OnStageIndexChanged -= HandleVoyageStageIndexChanged;
                _voyage.OnWaveIndexChanged -= HandleVoyageWaveIndexChanged;
                _voyage.OnStageComplete -= HandleVoyageStageComplete;

                _voyage = null;

                _netStageIds.Clear();
            }

            if (_voyage == null)
            {
                _voyage = new Voyage(_temperateSeaVoyageData);

                _netVoyageResult.value = VoyageResult.None;

                _voyage.OnStageIndexChanged += HandleVoyageStageIndexChanged;
                _voyage.OnWaveIndexChanged += HandleVoyageWaveIndexChanged;
                _voyage.OnStageComplete += HandleVoyageStageComplete;
                
                foreach (StageData data in _temperateSeaVoyageData.StageDatas)
                {
                    _netStageIds.Add(data.Id);
                }
            }

            _voyage.Continue();
        }

        private void HandleVoyageStageIndexChanged(int index)
        {
            _netStageIndex.value = index;
        }

        private void HandleVoyageWaveIndexChanged(int index)
        {
            _netWaveIndex.value = index;
        }

        private void HandleVoyageStageComplete()
        {
            if (_voyage.IsComplete)
            {
                _netVoyageResult.value = VoyageResult.Victory;
            }

            RaiseStageCompleteRpc();
        }

        [ObserversRpc]
        private void RaiseStageCompleteRpc()
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