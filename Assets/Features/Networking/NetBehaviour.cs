using UnityEngine;
using PurrNet;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Cameras;
using NoMoreFishAndChips.UI;
using ShinyOwl.Common;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.Saving;
using NoMoreFishAndChips.Instantiating;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Hitboxes;
using NoMoreFishAndChips.Effects;
using NoMoreFishAndChips.Voyages;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Rendering;

namespace NoMoreFishAndChips.Networking
{
    public abstract class NetBehaviour : NetworkBehaviour
    {
        private NetworkManager _networkManager;

        protected virtual void Awake()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
        }

        protected override void OnSpawned()
        {
            _networkManager.RaiseNetBehaviourSpawned(this);
        }

        protected override void OnDespawned()
        {
            _networkManager?.RaiseNetBehaviourDespawned(this);
        }
    }
}
