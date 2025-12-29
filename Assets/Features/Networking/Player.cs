using FishFlingers.Cameras;
using FishFlingers.Entities;
using FishFlingers.Scenes;
using PurrLobby;
using PurrNet;
using PurrNet.Transports;
using ShinyOwl.Common;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishFlingers.Networking
{
    public class Player : NetworkBehaviour, INetworkManagerListener
    {
        [SerializeField] private Character _humanPrefab;

        private CameraManager _cameraManager;
        private NetworkManager _networkManager;
        private SceneManager _sceneManager;

        private Character _human;

        protected override void OnEarlySpawn()
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _sceneManager = GameManager.Instance.Get<SceneManager>();
        }

        protected override void OnSpawned()
        {
            if (_networkManager.CurrentLobby.Properties[LobbyService.StartedKey] == true.ToString())
            {
                OnLobbyStart(_networkManager.CurrentLobby);
            }

            _networkManager.AddListener(this);
        }

        protected override void OnDespawned()
        {
            _networkManager.RemoveListener(this);
        }

        protected override void OnOwnerDisconnected(PlayerID ownerId)
        {
            Destroy(gameObject);
        }

        public void OnLobbyStart(Lobby lobby)
        {
            if (!isOwner)
            {
                return;
            }

            _ = SpawnHuman();
        }

        private async Task SpawnHuman()
        {
            while (!_sceneManager.IsSceneActive(EScene.Game))
            {
                await Task.Yield();
            }

            _human = Instantiate(_humanPrefab);

            _cameraManager.SetMode(new FollowCameraMode(_human.transform, new Vector3(0f, 3f, -5f)));
        }

        public void OnLobbyEnter(Lobby lobby) { }
        public void OnLobbyCreated(Lobby lobby) { }
        public void OnLobbyLeave() { }
        public void OnNetworkStarted(bool asServer) { }
        public void OnNetworkShutdown(bool asServer) { }
        public void OnClientConnectionState(ConnectionState state) { }
        public void OnPlayerJoined(PlayerID id, bool isReconnect, bool asServer) { }
        public void OnPlayerLeft(PlayerID id, bool asServer) { }
    }
}