using FishFlingers.Scenes;
using FishFlingers.States;
using PrimeTween;
using PurrNet;
using PurrNet.Prediction;
using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Networking.Predictions
{
    public interface IPredictionManagerListener
    { }

    public class PredictionManager : GameSystem<IPredictionManagerListener>, ISceneManagerListener
    {
        private PredictionManagerConfig _config;

        private SceneManager _sceneManager;

        private PurrNet.Prediction.PredictionManager _purrnetPredictionManager;

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.PredictionManagerConfig;

            _sceneManager = GameManager.Instance.Get<SceneManager>();
            _sceneManager.AddListener(this);

            base.Initialise(config);
        }

        public override void Shutdown()
        {
            _sceneManager?.RemoveListener(this);

            base.Shutdown();
        }

        public PredictedObjectID? Spawn(GameObject prefab, PlayerID? owner)
        {
            return _purrnetPredictionManager.hierarchy.Create(prefab, owner);
        }

        public void OnSceneSetActive(EScene previous, EScene current)
        {
            if (current == EScene.Game)
            {
                // Wish we could instantiate it ourselves, but the manager doesn't like that
                _purrnetPredictionManager = Object.FindFirstObjectByType<PurrNet.Prediction.PredictionManager>();
            }
            else if (previous == EScene.Game)
            {
                _purrnetPredictionManager = null;
            }
        }

        public void OnSceneLoaded(EScene scene, LoadSceneMode mode) { }
        public void OnSceneUnloaded(EScene scene) { }
    }
}
