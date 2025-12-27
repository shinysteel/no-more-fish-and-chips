using FishFlingers.Networking;
using ShinyOwl.Common.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FishFlingers.Scenes
{
    // Try to keep these 1:1 with scene names for clarity
    public enum EScene
    {
        Startup,
        Default,
        Game,
        EnvironmentMainMenu,
        EnvironmentGameplay,
    }

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

    public interface ISceneManagerListener
    {
        void OnSceneLoaded(EScene scene, LoadSceneMode mode);
        void OnSceneUnloaded(EScene scene);
    }

    public class SceneManager : GameSystem<ISceneManagerListener>
    {
        private SceneManagerConfig _config;

        private NetworkManager _networkManager;

        private Dictionary<EScene, string> _sceneNameMap;

        public override void Initialise(GameManagerConfig gameManagerConfig)
        {
            _config = gameManagerConfig.SceneManagerConfig;

            _networkManager = GameManager.Instance.Get<NetworkManager>();

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += HandleSceneUnloaded;

            _sceneNameMap = new();
            foreach (SceneMapping mapping in _config.SceneMappings)
            {
                _sceneNameMap.Add(mapping.Enum, mapping.Name);
            }

            base.Initialise(gameManagerConfig);
        }

        public override void Shutdown()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= HandleSceneUnloaded;

            base.Shutdown();
        }

        private string GetSceneName(EScene scene)
        {
            return _sceneNameMap[scene];
        }

        // We use 'sceneEnum' here so that it is not to be confused with unity's Scene struct
        private Scene GetScene(EScene sceneEnum)
        {
            return UnityEngine.SceneManagement.SceneManager.GetSceneByName(GetSceneName(sceneEnum));
        }

        private EScene GetScene(Scene scene)
        {
            return default;
        }

        public void LoadScene(EScene scene, LoadSceneMode mode = LoadSceneMode.Single)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(GetSceneName(scene), (UnityEngine.SceneManagement.LoadSceneMode)mode);
        }

        public AsyncOperationBridge LoadSceneAsync(EScene scene, LoadSceneMode mode = LoadSceneMode.Single, LoadSceneContext context = LoadSceneContext.Local)
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
                    op = _networkManager.LoadSceneAsync(name, (UnityEngine.SceneManagement.LoadSceneMode)mode);
                    break;
            }

            return new AsyncOperationBridge(op);
        }

        public AsyncOperationBridge UnloadSceneAsync(EScene scene, LoadSceneContext context = LoadSceneContext.Local)
        {
            string name = GetSceneName(scene);
            AsyncOperation op;

            switch (context)
            {
                default:
                case LoadSceneContext.Local:
                    op = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(name);
                    break;

                case LoadSceneContext.Networked:
                    op = _networkManager.UnloadSceneAsync(name);
                    break;
            }

            return new AsyncOperationBridge(op);
        }

        public void SetActiveScene(EScene scene)
        {
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(GetScene(scene));
        }

        private void HandleSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode) => Listeners.Dispatch(NotifyOnSceneLoaded, GetScene(scene), (LoadSceneMode)mode);
        private void HandleSceneUnloaded(Scene scene) => Listeners.Dispatch(NotifyOnSceneUnloaded, GetScene(scene));

        private void NotifyOnSceneLoaded(ISceneManagerListener listener, EScene scene, LoadSceneMode mode) => listener.OnSceneLoaded(scene, mode);
        private void NotifyOnSceneUnloaded(ISceneManagerListener listener, EScene scene) => listener.OnSceneUnloaded(scene);
    }
}