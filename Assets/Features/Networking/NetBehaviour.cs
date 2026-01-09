using UnityEngine;
using PurrNet;

namespace FishFlingers.Networking
{
    public abstract class NetBehaviour : NetworkBehaviour
    {
        protected NetworkManager _networkManager;

        protected override void OnInitializeModules()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
        }

        protected override void OnSpawned()
        {
            _networkManager.RaiseSpawned(this);
        }

        protected override void OnDespawned()
        {
            _networkManager.RaiseDespawned(this);
        }
    }
}
