using UnityEngine;
using PurrNet;
using FishFlingers.Entities;
using FishFlingers.Items;
using FishFlingers.Cameras;
using FishFlingers.UI;
using ShinyOwl.Common;

namespace FishFlingers.Networking
{
    public abstract class NetBehaviour : NetworkBehaviour
    {
        protected NetworkManager _networkManager;
        protected EntityManager _entityManager;
        protected LobbyManager _lobbyManager;
        protected ItemManager _itemManager;
        protected CameraManager _cameraManager;
        protected UIManager _uiManager;

        protected override void OnInitializeModules()
        {
            Debugger.Log(this, $"{name} OnInitializeModules");
            
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _lobbyManager = GameManager.Instance.Get<LobbyManager>();
            _itemManager = GameManager.Instance.Get<ItemManager>();
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _uiManager = GameManager.Instance.Get<UIManager>();
        }

        protected override void OnSpawned()
        {
            _networkManager.RaiseSpawned(this);
        }

        protected override void OnDespawned()
        {
            _networkManager.RaiseDespawned(this);
        }
    }
}
