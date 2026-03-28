using FishFlingers.Cameras;
using FishFlingers.Entities;
using FishFlingers.Saving;
using FishFlingers.Scenes;
using Newtonsoft.Json;
using PurrLobby;
using PurrNet;
using PurrNet.Transports;
using ShinyOwl.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace FishFlingers.Networking
{
    public class PurrnetPlayerSave
    {
        [JsonProperty] public int ItemInstanceIdCounter { get; private set; }
        [JsonProperty] public RaftPlayerSave RaftPlayer { get; private set; }

        public PurrnetPlayerSave()
        {
            ItemInstanceIdCounter = 0;
            RaftPlayer = new();
            RaftPlayer.ApplyDefaults();
        }

        public PurrnetPlayerSave(int itemInstanceIdCounter, RaftPlayerSave raftPlayer)
        {
            ItemInstanceIdCounter = itemInstanceIdCounter;
            RaftPlayer = raftPlayer;
        }
    }

    public class PurrnetPlayer : NetBehaviour, ISaveable
    {
        [SerializeField] private RaftPlayer _raftPlayerPrefab;

        private SyncVar<string> _netGuid = new SyncVar<string>(ownerAuth: true);
        private SyncVar<int> _netItemInstanceIdCounter = new SyncVar<int>(ownerAuth: true);
        private SyncVar<RaftPlayer> _netRaftPlayer = new SyncVar<RaftPlayer>(ownerAuth: true);

        protected override void OnSpawned()
        {
            base.OnSpawned();

            _instantiateManager.RaiseComponentInstantiated(this);

            if (isOwner)
            {
                _netGuid.value = _saveManager.UserSave.Guid;
            }
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _instantiateManager.RaiseComponentDestroyed(this);

            if (_networkManager.IsServer)
            {
                ((ISaveable)this).Save();
            }
        }

        // This solution is alright for making sure all items are unique across the session, but it will
        // break if a client stops their application and then rejoins, having their id counter reset to 0
        public string GetNextItemInstanceId()
        {
            return $"{_networkManager.LocalPlayerId}_{_netItemInstanceIdCounter.value++}";
        }

        public RaftPlayer CreateRaftPlayer()
        {
            _netRaftPlayer.value = _networkManager.Spawn(_raftPlayerPrefab, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition });
            return _netRaftPlayer;
        }

        [ServerRpc]
        private async Task<PurrnetPlayerSave> GetSaveRpc()
        {
            if (!_saveManager.GameSave.Players.ContainsKey(_netGuid))
            {
                _saveManager.GameSave.Players[_netGuid] = new();
            }

            return _saveManager.GameSave.Players[_netGuid];
        }

        async Task ISaveable.LoadAsync()
        {
            PurrnetPlayerSave save = await GetSaveRpc();

            _netItemInstanceIdCounter.value = save.ItemInstanceIdCounter;

            await _netRaftPlayer.value.LoadAsync(save.RaftPlayer);
        }

        void ISaveable.Save()
        {
            _saveManager.GameSave.Players[_netGuid] = new PurrnetPlayerSave(_netItemInstanceIdCounter.value, new RaftPlayerSave(_netRaftPlayer.value));
        }
    }
}