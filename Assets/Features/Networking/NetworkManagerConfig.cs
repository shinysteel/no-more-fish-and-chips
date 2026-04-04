using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishFlingers.Networking
{
    [CreateAssetMenu(fileName = "NetworkManagerConfig", menuName = "Configs/Managers/Networking/NetworkMangerConfig")]
    public class NetworkManagerConfig : ScriptableObject
    {
        [SerializeField] private PurrNet.NetworkManager _purrnetNetworkManagerPrefab;
        [SerializeField] private PurrnetPlayer _purrnetPlayerPrefab;
        [SerializeField] private ushort _udpServerPort = 5001;
        [SerializeField] private ushort _steamServerPort = 5003;

        public PurrNet.NetworkManager PurrnetNetworkManagerPrefab => _purrnetNetworkManagerPrefab;
        public PurrnetPlayer PurrnetPlayerPrefab => _purrnetPlayerPrefab;
        public ushort UDPServerPort => _udpServerPort;
        public ushort SteamServerPort => _steamServerPort;
    }
}