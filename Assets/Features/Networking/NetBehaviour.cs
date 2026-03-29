using UnityEngine;
using PurrNet;
using FishFlingers.Entities;
using FishFlingers.Items;
using FishFlingers.Cameras;
using FishFlingers.UI;
using ShinyOwl.Common;
using FishFlingers.States;
using FishFlingers.Saving;
using FishFlingers.Instantiating;

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
        protected SaveManager _saveManager;
        protected InstantiateManager _instantiateManager;

        protected override void OnInitializeModules()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _lobbyManager = GameManager.Instance.Get<LobbyManager>();
            _itemManager = GameManager.Instance.Get<ItemManager>();
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _uiManager = GameManager.Instance.Get<UIManager>();
            _saveManager = GameManager.Instance.Get<SaveManager>();
            _instantiateManager = GameManager.Instance.Get<InstantiateManager>();
        }

        protected override void OnSpawned()
        {
            _networkManager.RaiseNetBehaviourSpawned(this);
        }

        protected override void OnDespawned()
        {
            _networkManager?.RaiseNetBehaviourDespawned(this);
        }
    }
}
