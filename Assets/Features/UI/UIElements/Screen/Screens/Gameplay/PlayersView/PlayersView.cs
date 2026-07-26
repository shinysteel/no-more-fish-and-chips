using NoMoreFishAndChips.Networking;
using UnityEngine;
using PurrNet;
using ShinyOwl.Common.Utils;
using System.Collections.Generic;
using NoMoreFishAndChips.Pools;
using UnityEngine.UI;
using ShinyOwl.Common;

using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;

namespace NoMoreFishAndChips.UI
{
    public class PlayersView : MonoBehaviour, INetworkManagerListener
    {
        [SerializeField] private PlayerEntry _playerEntryPrefab;

        private NetworkManager _networkManager;
        private PoolManager _poolManager;

        private List<PlayerEntry> _playerEntries = new();

        private void Awake()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();

            Refresh();

            _networkManager.AddListener(this);
        }
        
        private void OnDestroy()
        {
            _networkManager?.RemoveListener(this);
        }

        void INetworkManagerListener.OnNetBehaviourSpawned(NetBehaviour behaviour)
        {
            if (behaviour is not PurrnetPlayer)
            {
                return;
            }

            Refresh();
        }

        void INetworkManagerListener.OnNetBehaviourDespawned(NetBehaviour behaviour)
        {
            if (behaviour is not PurrnetPlayer)
            {
                return;
            }

            Refresh();   
        }

        private void Refresh()
        {
            Utils.Collections.ResizeList(_playerEntries, _networkManager.PurrnetPlayers.Count,
                createElement: () => _poolManager.GetTypedPoolable<PlayerEntry>(new SpawnParams() { Parent = transform }),
                removeElement: (PlayerEntry entry) => _poolManager.ReturnTypedPoolable(entry),
                processElement: (PlayerEntry entry, int index) => entry.Setup(_networkManager.PurrnetPlayers[index]));
        }
    }
}