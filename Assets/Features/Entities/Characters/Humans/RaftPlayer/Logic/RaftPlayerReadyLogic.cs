using PurrNet;
using UnityEngine;
using System;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerReadyLogic : RaftPlayerLogic
    {
        private SyncVar<bool> _netIsReady;

        public bool IsReady => _netIsReady.value;

        public event Action<bool> OnIsReadyChanged;

        public RaftPlayerReadyLogic(RaftPlayer player, SyncVar<bool> netIsReady) : base(player)
        {
            _netIsReady = netIsReady;

            _netIsReady.onChanged += HandleNetIsReadyChanged;
        }

        public override void OnDespawned()
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