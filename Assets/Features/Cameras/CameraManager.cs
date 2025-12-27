using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishFlingers.Cameras
{
    public interface ICameraMode
    {
        void Enter(Camera camera);
        void LateUpdate(Camera camera);
        void Exit(Camera camera);
    }

    public interface ICameraManagerListener
    { }

    public class CameraManager : GameSystem<ICameraManagerListener>
    {
        private CameraManagerConfig _config;

        private Camera _camera;
        private ICameraMode _mode;

        public override void Initialise(GameManagerConfig gameManagerConfig)
        {
            _config = gameManagerConfig.CameraManagerConfig;

            _camera = Object.Instantiate(_config.GameCameraPrefab);

            Object.DontDestroyOnLoad(_camera.gameObject);

            base.Initialise(gameManagerConfig);
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }

        public override void Update()
        {
            _mode?.LateUpdate(_camera);
        }

        public void SetMode(ICameraMode mode)
        {
            _mode?.Exit(_camera);
            _mode = mode;
            _mode?.Enter(_camera);
        }
    }
}