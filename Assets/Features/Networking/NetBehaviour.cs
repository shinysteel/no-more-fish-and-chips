using UnityEngine;
using PurrNet;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Cameras;
using NoMoreFishAndChips.UI;
using ShinyOwl.Common;
using NoMoreFishAndChips.States;
using NoMoreFishAndChips.Saving;
using NoMoreFishAndChips.Instantiating;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Hitboxes;
using NoMoreFishAndChips.Effects;

namespace NoMoreFishAndChips.Networking
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
        protected PoolManager _poolManager;
        protected AudioManager _audioManager;
        protected HitboxManager _hitboxManager;
        protected EffectManager _effectManager;

        protected virtual void Awake()
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _lobbyManager = GameManager.Instance.Get<LobbyManager>();
            _itemManager = GameManager.Instance.Get<ItemManager>();
            _cameraManager = GameManager.Instance.Get<CameraManager>();
            _uiManager = GameManager.Instance.Get<UIManager>();
            _saveManager = GameManager.Instance.Get<SaveManager>();
            _instantiateManager = GameManager.Instance.Get<InstantiateManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();
            _audioManager = GameManager.Instance.Get<AudioManager>();
            _hitboxManager = GameManager.Instance.Get<HitboxManager>();
            _effectManager = GameManager.Instance.Get<EffectManager>();
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
