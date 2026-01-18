using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishFlingers.Cameras
{
    public interface ICameraMode
    {
        void Enter(Camera camera);
        void LateTick(Camera camera);
        void Exit(Camera camera);
    }

    public interface ICameraManagerListener
    { }

    public class CameraManager : GameSystem<ICameraManagerListener>
    {
        private CameraManagerConfig _config;

        private Camera _mainCamera;
        public Camera MainCamera => _mainCamera;

        private ICameraMode _mode;

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.CameraManagerConfig;

            _mainCamera = Object.Instantiate(_config.MainCameraPrefab);

            Object.DontDestroyOnLoad(_mainCamera.gameObject);

            base.Initialise(config);
        }

        public override void LateTick()
        {
            _mode?.LateTick(_mainCamera);
        }

        public void SetMode(ICameraMode mode)
        {
            _mode?.Exit(_mainCamera);
            _mode = mode;
            _mode?.Enter(_mainCamera);
        }
    }
}