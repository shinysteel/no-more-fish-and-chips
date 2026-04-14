using FishFlingers.Networking;
using PurrNet;
using System;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class RaftPlayerOpenNetBehaviourLogic
    {
        private SyncVar<NetBehaviour> _netBehaviour;

        private NetBehaviour _behaviour;
        public NetBehaviour Behaviour => _behaviour;

        public event Action<NetBehaviour> OnBehaviourChanged;

        public RaftPlayerOpenNetBehaviourLogic(SyncVar<NetBehaviour> netBehaviour)
        {
            _netBehaviour = netBehaviour;

            _netBehaviour.onChanged += HandleNetBehaviourChanged;
        }
        ~RaftPlayerOpenNetBehaviourLogic()
        {
            _netBehaviour.onChanged -= HandleNetBehaviourChanged;
        }

        private void HandleNetBehaviourChanged(NetBehaviour behaviour)
        {
            _behaviour = behaviour;

            OnBehaviourChanged?.Invoke(_behaviour);
        }
    }
}