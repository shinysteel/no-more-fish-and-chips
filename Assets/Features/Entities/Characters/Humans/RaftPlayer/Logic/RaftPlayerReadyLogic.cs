using PurrNet;
using UnityEngine;
using System;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerReadyLogic
    {
        private SyncVar<bool> _netIsReady;

        public bool IsReady => _netIsReady.value;

        public event Action<bool> OnIsReadyChanged;

        public RaftPlayerReadyLogic(SyncVar<bool> netIsReady)
        {
            _netIsReady = netIsReady;

            _netIsReady.onChanged += HandleNetIsReadyChanged;
        }

        ~RaftPlayerReadyLogic()
        {
            _netIsReady.onChanged -= HandleNetIsReadyChanged;
        }

        private void HandleNetIsReadyChanged(bool ready)
        {
            OnIsReadyChanged?.Invoke(ready);
        }

        public void SetNetIsReady(bool ready)
        {
            _netIsReady.value = ready;
        }
    }
}