using PurrNet;
using System;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class RaftPlayerOpenNetworkIdLogic
    {
        private SyncVar<NetworkID> _netId;

        private NetworkID _id;
        public NetworkID Id => _id;

        public event Action<NetworkID> OnIdChanged;

        public RaftPlayerOpenNetworkIdLogic(SyncVar<NetworkID> netId)
        {
            _netId = netId;

            _netId.onChanged += HandleNetIdChanged;
        }
        ~RaftPlayerOpenNetworkIdLogic()
        {
            _netId.onChanged -= HandleNetIdChanged;
        }

        private void HandleNetIdChanged(NetworkID id)
        {
            _id = id;

            OnIdChanged?.Invoke(_id);
        }
    }
}