using UnityEngine;

namespace NoMoreFishAndChips.Networking
{
    [CreateAssetMenu(fileName = "LobbyManagerConfig", menuName = "Configs/Managers/Networking/LobbyManagerConfig")]
    public class LobbyManagerConfig : ScriptableObject
    {
        [SerializeField] private ushort _broadcastPort = 5962;

        public ushort BroadcastPort => _broadcastPort;
    }
}