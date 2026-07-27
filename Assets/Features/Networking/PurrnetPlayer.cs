using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Saving;
using PurrNet;
using ShinyOwl.Common;
using Steamworks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using EntityId = NoMoreFishAndChips.Entities.EntityId;

namespace NoMoreFishAndChips.Networking
{
    public class PurrnetPlayer : NetBehaviour, ISaveable
    {
        private SyncVar<string> _netGuid = new SyncVar<string>(ownerAuth: true);
        private SyncVar<int> _netSaveId = new SyncVar<int>(ownerAuth: true);
        private SyncVar<int> _netItemInstanceIdCounter = new SyncVar<int>(ownerAuth: true);
        private SyncVar<string> _netUsername = new SyncVar<string>(ownerAuth: true);
        private SyncLazyRef<RaftPlayer> _netRaftPlayer = new SyncLazyRef<RaftPlayer>(ownerAuth: true);

        public int SaveId => _netSaveId.value;  
        public int ItemInstanceIdCounter => _netItemInstanceIdCounter.value;
        public RaftPlayer RaftPlayer => _netRaftPlayer.value;
        public string Username => _netUsername.value;

        public event Action<string> OnUsernameChanged;

        protected override void OnSpawned()
        {
            base.OnSpawned();

            _instantiateManager.RaiseComponentInstantiated(this);

            _netUsername.onChanged += HandleNetUsernameChanged;

            if (isOwner)
            {
                _netGuid.value = _saveManager.UserSave.Guid;

                if (_lobbyManager.CurrentLobby.Service == ELobbyService.Steam)
                {
                    _netUsername.value = SteamFriends.GetPersonaName();
                }
            }
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            _instantiateManager.RaiseComponentDestroyed(this);

            _netUsername.onChanged -= HandleNetUsernameChanged;

            if (_networkManager.IsServer)
            {
                ((ISaveable)this).Save();
            }
        }

        private void HandleNetUsernameChanged(string username)
        {
            OnUsernameChanged?.Invoke(username);
        }

        // Every instance of an inventory item needs a unique id, and we can guarantee this by combining the
        // player's id with a local counter
        public string GetNextNetItemInstanceId()
        {
            return $"{_netSaveId.value}_{_netItemInstanceIdCounter.value++}";
        }

        public RaftPlayer CreateRaftPlayer()
        {
            _netRaftPlayer.value = (RaftPlayer)_entityManager.Spawn(EntityId.RaftPlayer, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition });
            return _netRaftPlayer.value;
        }

        public void SetNetSaveId(int id)
        {
            _netSaveId.value = id;
        }

        public void SetNetItemInstanceIdCounter(int counter)
        {
            _netItemInstanceIdCounter.value = counter;
        }

        public void SetNetUsername(string username)
        {
            _netUsername.value = username;
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
            // Clients can join while the host is still initialising, and so they will be in the collection of Saveables
            // to load on the server. We can just return here knowing that clients will load their player themselves
            if (!isOwner)
            {
                return;
            }

            PurrnetPlayerSave save = await GetSaveRpc();

            await save.LoadToAsync(this);

            if (_lobbyManager.CurrentLobby.Service == ELobbyService.LAN)
            {
                _netUsername.value = $"Player {_netSaveId.value + 1}";
            }
        }

        void ISaveable.Save()
        {
            _saveManager.GameSave.Players[_netGuid.value].SaveFrom(this);
        }
    }
}