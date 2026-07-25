using NoMoreFishAndChips.Networking;
using PurrNet;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerOpenNetBehaviourLogic
    {
        private SyncVar<NetBehaviour> _netBehaviour;

        public NetBehaviour Behaviour => _netBehaviour.value;

        public event Action<NetBehaviour, NetBehaviour> OnChanged;
        
        public RaftPlayerOpenNetBehaviourLogic(SyncVar<NetBehaviour> netBehaviour)
        {
            _netBehaviour = netBehaviour;

            HandleNetBehaviourChanged(null, _netBehaviour.value);

            _netBehaviour.onChangedWithOld += HandleNetBehaviourChanged;
        }
        ~RaftPlayerOpenNetBehaviourLogic()
        {
            _netBehaviour.onChangedWithOld -= HandleNetBehaviourChanged;
        }

        private void HandleNetBehaviourChanged(NetBehaviour previous, NetBehaviour current)
        {
            OnChanged?.Invoke(previous, current);
        }
    }
}