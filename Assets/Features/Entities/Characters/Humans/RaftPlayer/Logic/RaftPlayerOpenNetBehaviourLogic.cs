using NoMoreFishAndChips.Networking;
using PurrNet;
using System;
using System.Globalization;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerOpenNetBehaviourLogic : RaftPlayerLogic
    {
        private SyncVar<NetBehaviour> _netBehaviour;

        public NetBehaviour Behaviour => _netBehaviour.value;

        public event Action<NetBehaviour, NetBehaviour> OnChanged;
        
        public RaftPlayerOpenNetBehaviourLogic(RaftPlayer player, SyncVar<NetBehaviour> netBehaviour) : base(player)
        {
            _netBehaviour = netBehaviour;

            HandleNetBehaviourChanged(null, _netBehaviour.value);

            _netBehaviour.onChangedWithOld += HandleNetBehaviourChanged;
        }

        public override void OnDespawned()
        {
            _netBehaviour.onChangedWithOld -= HandleNetBehaviourChanged;
        }

        private void HandleNetBehaviourChanged(NetBehaviour previous, NetBehaviour current)
        {
            OnChanged?.Invoke(previous, current);
        }

        public void SetNetBehaviour(NetBehaviour behaviour)
        {
            if (!_player.isOwner)
            {
                return;
            }

            _netBehaviour.value = behaviour;
        }
    }
}