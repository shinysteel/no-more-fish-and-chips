using FishFlingers.Networking;
using PurrNet;
using PurrNet.Modules;
using PurrNet.Transports;
using ShinyOwl.Common;
using ShinyOwl.Common.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using NetworkManager = FishFlingers.Networking.NetworkManager;

namespace FishFlingers.Scenes
{
    // Try to keep these 1:1 with scene names for clarity
    // If changing the order, don't forget to update SceneManagerConfig
    public enum EScene
    {
        Startup             ,
        Default             ,
        Game                ,
        EnvironmentMainMenu ,
        EnvironmentGameplay ,
    }

    // A duplicate of UnityEngine.SceneManagement.LoadSceneMode to stop the namespace being added
    // and causing conflicts when using our custom SceneManager
    public enum LoadSceneMode
    {
        Single   ,
        Additive , 
    }

    public enum LoadSceneContext
    {
        Local     ,
        Networked ,
    }

    // EScene.DontDestroyOnLoad existing doesn't make sense, given it wouldn't interact
    // with most of SceneManager's functionality. That being said, it's still an option
    // to consider for instantiating something, and that's why this exists
    public struct SpawnScene
    {
        private EScene? _scene;
        private bool _dontDestroyOnLoad;

        public static SpawnScene ActiveScene() => new SpawnScene(null, false);
        public static SpawnScene Scene(EScene scene) => new SpawnScene(scene, false);
        public static SpawnScene DontDestroyOnLoad() => new SpawnScene(null, true);

        private SpawnScene(EScene? scene, bool dontDestroyOnLoad)
        {
            _scene = scene;
            _dontDestroyOnLoad = dontDestroyOnLoad;
        }

        public Scene Get()
        {
            SceneManager sceneManager = GameManager.Instance.Get<SceneManager>();

            if (_dontDestroyOnLoad)
            {
                return sceneManager.GetDontDestroyOnLoadScene();
            }
            else if (_scene.HasValue)
            {
                return sceneManager.GetScene(_scene.Value);
            }
            else
            {
                return sceneManager.GetActiveScene();
            }
        }
    }

    public interface ISceneManagerListener
    {
        void OnSceneLoaded(EScene scene, LoadSceneMode mode);
        void OnSceneUnloaded(EScene scene);
        void OnNetworkedSceneLoaded(EScene scene, bool asServer);
        void OnNetworkedSceneUnloaded(EScene scene, bool asServer);
        void OnPlayerLoadedScene(PlayerID playerId, EScene scene, bool asServer);
        void OnPlayerUnloadedScene(PlayerID playerId, EScene scene, bool asServer);
        void OnActiveSceneChanged(EScene previous, EScene current);
    }

    public class SceneManager : GameSystem<ISceneManagerListener>, INetworkManagerListener
    {
        private SceneManagerConfig _config;

        private NetworkManager _networkManager;

        private ScenesModule _scenesModule;
        private ScenePlayersModule _scenePlayersModule;

        private Dictionary<EScene, string> _sceneNameMap;

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.SceneManagerConfig;

            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _networkManager.AddListener(this);

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += HandleSceneUnloaded;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += HandleActiveSceneChanged;

            _sceneNameMap = new();
            foreach (SceneMapping mapping in _config.SceneMappings)
            {
                _sceneNameMap.Add(mapping.Enum, mapping.Name);
            }

            base.Initialise(config);
        }

        public override void Shutdown()
        {
            _networkManager?.RemoveListener(this);

            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= HandleActiveSceneChanged;

            base.Shutdown();
        }

        private EScene GetSceneEnum(SceneID id)
        {
            _scenesModule.TryGetSceneState(id, out SceneState state);
            return GetSceneEnum(state.scene);
        }

        public EScene GetSceneEnum(Scene scene)
        {
            return _sceneNameMap.FirstOrDefault(kvp => kvp.Value == scene.name).Key;
        }

        // GetSceneByName is not able to find the DontDestroyOnLoad scene, so this seems to be the solution for now
        public Scene GetDontDestroyOnLoadScene()
        {
            return GameManager.Instance.gameObject.scene;
        }

        public Scene GetScene(EScene scene)
        {
            return UnityEngine.SceneManagement.SceneManager.GetSceneByName(GetSceneName(scene));
        }

        public string GetSceneName(EScene scene)
        {
            return _sceneNameMap[scene];
        }

        private EScene GetActiveSceneEnum()
        {
            return GetSceneEnum(GetActiveScene());
        }

        public Scene GetActiveScene()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        }

        public void SetActiveScene(EScene scene)
        {
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(GetScene(scene));
        }

        // We use the scene struct, since the DontDestroyOnLoad scene can't be referenced via enum
        public void MoveGameObjectToScene(GameObject obj, Scene scene)
        {
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj, scene);
        }

        public bool IsSceneActive(EScene scene)
        {
            return GetActiveScene().name == GetSceneName(scene);
        }

        public bool IsSceneLoaded(EScene scene)
        {
            return GetScene(scene).isLoaded;
        }

        // Purrnet doesn't implement non async scene loading, so LoadScene can only be done locally
        public void LoadScene(EScene scene, LoadSceneMode mode = LoadSceneMode.Single)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(GetSceneName(scene), (UnityEngine.SceneManagement.LoadSceneMode)mode);
        }

        public AsyncOperationBridge LoadSceneAsync(EScene scene, LoadSceneMode mode, LoadSceneContext context)
        {
            string name = GetSceneName(scene);
            AsyncOperation op;

            switch (context)
            {
                default:
                case LoadSceneContext.Local:
                    op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name, (UnityEngine.SceneManagement.LoadSceneMode)mode);
                    break;

                case LoadSceneContext.Networked:
                    op = _scenesModule.LoadSceneAsync(GetSceneName(scene), (UnityEngine.SceneManagement.LoadSceneMode)mode);
                    break;
            }

            return new AsyncOperationBridge(op);
        }

        public AsyncOperationBridge UnloadSceneAsync(EScene scene, LoadSceneContext context)
        {
            string name = GetSceneName(scene);
            AsyncOperation op;

            switch (context)
            {
                default:
                case LoadSceneContext.Local:
                    op = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(GetSceneName(scene));
                    break;

                case LoadSceneContext.Networked:
                    op = _scenesModule.UnloadSceneAsync(GetSceneName(scene));
                    break;
            }

            return new AsyncOperationBridge(op);
        }

        private void HandleSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode) => Listeners.Dispatch(NotifyOnSceneLoaded, GetSceneEnum(scene), (LoadSceneMode)mode);
        private void HandleSceneUnloaded(Scene scene) => Listeners.Dispatch(NotifyOnSceneUnloaded, GetSceneEnum(scene));
        private void HandleNetworkedSceneLoaded(SceneID id, bool asServer) => Listeners.Dispatch(NotifyOnNetworkedSceneLoaded, GetSceneEnum(id), asServer);
        private void HandleNetworkedSceneUnloaded(SceneID id, bool asServer) => Listeners.Dispatch(NotifyOnNetworkedSceneUnloaded, GetSceneEnum(id), asServer);
        private void HandlePlayerLoadedScene(PlayerID playerId, SceneID sceneId, bool asServer) => Listeners.Dispatch(NotifyOnPlayerLoadedScene, playerId, GetSceneEnum(sceneId), asServer);
        private void HandlePlayerUnloadedScene(PlayerID playerId, SceneID sceneId, bool asServer) => Listeners.Dispatch(NotifyOnPlayerUnloadedScene, playerId, GetSceneEnum(sceneId), asServer);
        private void HandleActiveSceneChanged(Scene previous, Scene current) => Listeners.Dispatch(NotifyOnActiveSceneChanged, GetSceneEnum(previous), GetSceneEnum(current));

        private void NotifyOnSceneLoaded(ISceneManagerListener listener, EScene scene, LoadSceneMode mode) => listener.OnSceneLoaded(scene, mode);
        private void NotifyOnSceneUnloaded(ISceneManagerListener listener, EScene scene) => listener.OnSceneUnloaded(scene);
        private void NotifyOnNetworkedSceneLoaded(ISceneManagerListener listener, EScene scene, bool asServer) => listener.OnNetworkedSceneLoaded(scene, asServer);
        private void NotifyOnNetworkedSceneUnloaded(ISceneManagerListener listener, EScene scene, bool asServer) => listener.OnNetworkedSceneUnloaded(scene, asServer);
        private void NotifyOnPlayerLoadedScene(ISceneManagerListener listener, PlayerID playerId, EScene scene, bool asServer) => listener.OnPlayerLoadedScene(playerId, scene, asServer);
        private void NotifyOnPlayerUnloadedScene(ISceneManagerListener listener, PlayerID playerId, EScene scene, bool asServer) => listener.OnPlayerUnloadedScene(playerId, scene, asServer);
        private void NotifyOnActiveSceneChanged(ISceneManagerListener listener, EScene previous, EScene current) => listener.OnActiveSceneChanged(previous, current);

        public void OnNetworkStarted(bool asServer)
        {
            // Since the host is also the server, we need to stop them subscribing twice
            if (asServer)
            {
                return;
            }

            _scenesModule = _networkManager.GetModule<ScenesModule>(_networkManager.IsServer);
            _scenePlayersModule = _networkManager.GetModule<ScenePlayersModule>(_networkManager.IsServer);

            _scenesModule.onSceneLoaded += HandleNetworkedSceneLoaded;
            _scenesModule.onSceneUnloaded += HandleNetworkedSceneUnloaded;

            _scenePlayersModule.onPlayerLoadedScene += HandlePlayerLoadedScene;
            _scenePlayersModule.onPlayerUnloadedScene += HandlePlayerUnloadedScene;
        }

        public void OnNetworkShutdown(bool asServer) 
        {
            if (asServer)
            {
                return;
            }

            if (_scenesModule != null)
            {
                _scenesModule.onSceneLoaded -= HandleNetworkedSceneLoaded;
                _scenesModule.onSceneUnloaded -= HandleNetworkedSceneUnloaded;
                _scenesModule = null;
            }

            if (_scenePlayersModule != null)
            {
                _scenePlayersModule.onPlayerLoadedScene -= HandlePlayerLoadedScene;
                _scenePlayersModule.onPlayerUnloadedScene -= HandlePlayerUnloadedScene;
                _scenePlayersModule = null;
            }
        }

        public void OnNetworkSpawn(NetBehaviour behaviour) { }
        public void OnNetworkDespawn(NetBehaviour behaviour) { }
        public void OnClientConnectionState(ConnectionState state) { }
        public void OnPlayerJoined(PlayerID id, bool isReconnect, bool asServer) { }
        public void OnPlayerLeft(PlayerID id, bool asServer) { }
    }
}