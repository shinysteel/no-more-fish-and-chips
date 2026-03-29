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
using EntityId = FishFlingers.Entities.EntityId;

namespace FishFlingers.Networking
{
    public class PurrnetPlayerSave
    {
        [JsonProperty] public int SaveId { get; private set; }
        [JsonProperty] public int ItemInstanceIdCounter { get; private set; } = 0;
        [JsonProperty] public RaftPlayerSave RaftPlayer { get; private set; } = new();

        public PurrnetPlayerSave()
        { }

        public PurrnetPlayerSave(int saveId, int itemInstanceIdCounter, RaftPlayerSave raftPlayer)
        {
            SaveId = saveId;
            ItemInstanceIdCounter = itemInstanceIdCounter;
            RaftPlayer = raftPlayer;
        }

        public void ApplyDefaults()
        {
            RaftPlayer.ApplyDefaults();
        }

        // We can't safely reference any managers in the default constructor, so this exists
        public void SetSaveId(int id)
        {
            SaveId = id;
        }
    }

    public class PurrnetPlayer : NetBehaviour, ISaveable
    {
        private SyncVar<string> _netGuid = new SyncVar<string>(ownerAuth: true);
        private SyncVar<int> _netSaveId = new SyncVar<int>(ownerAuth: true);
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
            return $"{_netSaveId.value}_{_netItemInstanceIdCounter.value++}";
        }

        public RaftPlayer CreateRaftPlayer()
        {
            _netRaftPlayer.value = (RaftPlayer)_entityManager.Spawn(EntityId.RaftPlayer, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition });
            return _netRaftPlayer;
        }

        [ServerRpc]
        private async Task<PurrnetPlayerSave> GetSaveRpc()
        {
            // Syncvars won't be ready if this is requested as the host is initialising
            while (!isSpawned)
            {
                await Task.Yield();
            }

            if (!_saveManager.GameSave.Players.ContainsKey(_netGuid))
            {
                _saveManager.GameSave.Players[_netGuid] = new();
                _saveManager.GameSave.Players[_netGuid].ApplyDefaults();
                _saveManager.GameSave.Players[_netGuid].SetSaveId(_saveManager.GameSave.Players.Count - 1);
            }

            return _saveManager.GameSave.Players[_netGuid];
        }

        async Task ISaveable.LoadAsync()
        {
            PurrnetPlayerSave save = await GetSaveRpc();

            // The raft player may not be created yet
            while (_netRaftPlayer.value == null)
            {
                await Task.Yield();
            }

            _netSaveId.value = save.SaveId;
            _netItemInstanceIdCounter.value = save.ItemInstanceIdCounter;

            await _netRaftPlayer.value.LoadAsync(save.RaftPlayer);
        }

        void ISaveable.Save()
        {
            _saveManager.GameSave.Players[_netGuid] = new PurrnetPlayerSave(_netSaveId.value, _netItemInstanceIdCounter.value, new RaftPlayerSave(_netRaftPlayer.value));
        }
    }
}