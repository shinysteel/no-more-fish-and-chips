using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishFlingers.Networking;
using FishFlingers.Cameras;
using FishFlingers.States;
using FishFlingers.UI;
using FishFlingers.Pools;
using FishFlingers.UI.Transitions;
using FishFlingers.Scenes;
using FishFlingers.Localisation;
using FishFlingers.Items;
using FishFlingers.Entities;
using FishFlingers.Saving;
using FishFlingers.Instantiating;
using FishFlingers.Effects;
using FishFlingers.Hitboxes;
using FishFlingers.Audio;

[CreateAssetMenu(fileName = "GameManagerConfig", menuName = "Configs/Managers/GameManagerConfig")]
public class GameManagerConfig : ScriptableObject
{
    [SerializeField] private SaveManagerConfig _saveManagerConfig;
    [SerializeField] private InstantiateManagerConfig _instantiateManagerConfig;
    [SerializeField] private SteamManagerConfig _steamManagerConfig;
    [SerializeField] private NetworkManagerConfig _networkManagerConfig;
    [SerializeField] private CameraManagerConfig _cameraManagerConfig;
    [SerializeField] private StateManagerConfig _stateManagerConfig;
    [SerializeField] private UIManagerConfig _uiManagerConfig;
    [SerializeField] private PoolManagerConfig _poolManagerConfig;
    [SerializeField] private TransitionManagerConfig _transitionManagerConfig;
    [SerializeField] private SceneManagerConfig _sceneManagerConfig;
    [SerializeField] private DebugManagerConfig _debugManagerConfig;
    [SerializeField] private LocalisationManagerConfig _localisationManagerConfig;
    [SerializeField] private LobbyManagerConfig _lobbyManagerConfig;
    [SerializeField] private ItemManagerConfig _itemManagerConfig;
    [SerializeField] private EntityManagerConfig _entityManagerConfig;
    [SerializeField] private EffectManagerConfig _effectManagerConfig;
    [SerializeField] private HitboxManagerConfig _hitboxManagerConfig;
    [SerializeField] private AudioManagerConfig _audioManagerConfig;

    public SaveManagerConfig SaveManagerConfig => _saveManagerConfig;
    public InstantiateManagerConfig InstantiateManagerConfig => _instantiateManagerConfig;
    public SteamManagerConfig SteamManagerConfig => _steamManagerConfig;
    public NetworkManagerConfig NetworkManagerConfig => _networkManagerConfig;
    public CameraManagerConfig CameraManagerConfig => _cameraManagerConfig;
    public StateManagerConfig StateManagerConfig => _stateManagerConfig;
    public UIManagerConfig UIManagerConfig => _uiManagerConfig;
    public PoolManagerConfig PoolManagerConfig => _poolManagerConfig;
    public TransitionManagerConfig TransitionManagerConfig => _transitionManagerConfig;
    public SceneManagerConfig SceneManagerConfig => _sceneManagerConfig;
    public DebugManagerConfig DebugManagerConfig => _debugManagerConfig;
    public LocalisationManagerConfig LocalisationManagerConfig => _localisationManagerConfig;
    public LobbyManagerConfig LobbyManagerConfig => _lobbyManagerConfig;
    public ItemManagerConfig ItemManagerConfig => _itemManagerConfig;
    public EntityManagerConfig EntityManagerConfig => _entityManagerConfig;
    public EffectManagerConfig EffectManagerConfig => _effectManagerConfig;
    public HitboxManagerConfig HitboxManagerConfig => _hitboxManagerConfig;
    public AudioManagerConfig AudioManagerConfig => _audioManagerConfig;
}