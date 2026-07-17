using NoMoreFishAndChips.Networking;
using PurrNet;
using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerOpenNetBehaviourLogic
    {
        private SyncVar<NetBehaviour> _netBehaviour;

        private NetBehaviour _behaviour;
        public NetBehaviour Behaviour => _behaviour;

        public event Action<NetBehaviour, NetBehaviour> OnChanged;
        
        public RaftPlayerOpenNetBehaviourLogic(SyncVar<NetBehaviour> netBehaviour)
        {
            _netBehaviour = netBehaviour;

            HandleNetBehaviourChanged(_netBehaviour);

            _netBehaviour.onChanged += HandleNetBehaviourChanged;
        }
        ~RaftPlayerOpenNetBehaviourLogic()
        {
            _netBehaviour.onChanged -= HandleNetBehaviourChanged;
        }

        private void HandleNetBehaviourChanged(NetBehaviour behaviour)
        {
            NetBehaviour old = _behaviour;
            _behaviour = behaviour;

            OnChanged?.Invoke(old, _behaviour);
        }
    }
}