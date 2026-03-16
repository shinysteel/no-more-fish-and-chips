using FishFlingers.Cameras;
using FishFlingers.Scenes;
using PurrLobby;
using PurrNet;
using PurrNet.Transports;
using ShinyOwl.Common;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using FishFlingers.Entities;
using FishFlingers.Saving;

namespace FishFlingers.Networking
{
    public class PurrnetPlayer : NetBehaviour
    {
        [SerializeField] private RaftPlayer _raftPlayerPrefab;

        private SyncVar<string> _guid = new SyncVar<string>(ownerAuth: true);
        private SyncVar<RaftPlayer> _raftPlayer = new SyncVar<RaftPlayer>(ownerAuth: true);

        public string Guid => _guid;
        public RaftPlayer RaftPlayer => _raftPlayer;

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (isOwner)
            {
                _guid.value = _saveManager.UserSave.Guid;
            }
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();

            if (_networkManager.IsServer)
            {
                _saveManager.SaveRaftPlayer(_guid, _raftPlayer);
            }
        }

        public RaftPlayer CreateRaftPlayer()
        {
            _raftPlayer.value = _networkManager.Spawn(_raftPlayerPrefab, new SpawnParams() { Position = NetworkManager.HiddenSpawnPosition });
            return _raftPlayer;
        }

        public async Task LoadRaftPlayerAsync()
        {
            await _raftPlayer.value.LoadDataAsync(_guid);
        }
    }
}