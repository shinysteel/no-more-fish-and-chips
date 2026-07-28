using NoMoreFishAndChips.Networking;
using PurrNet;
using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class Island : NetBehaviour
    {
        [SerializeField] private NetworkRigidbody _networkRigidbody;
        public NetworkRigidbody NetworkRigidbody => _networkRigidbody;
    }
}