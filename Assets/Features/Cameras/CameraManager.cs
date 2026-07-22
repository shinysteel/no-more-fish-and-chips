using NoMoreFishAndChips.Instantiating;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using System.Threading.Tasks;
using ShinyOwl.Common;

namespace NoMoreFishAndChips.Cameras
{
    public interface ICameraManagerListener
    { }

    public class CameraManager : GameSystem<ICameraManagerListener>
    {
        private CameraManagerConfig _config;
        public CameraManagerConfig Config => _config;

        private CinemachineBrain _cinemachineBrain;
        public CinemachineBrain CinemachineBrain => _cinemachineBrain;

        private ICameraMode _blendingMode;
        private ICameraMode _currentMode;

        public override void InitialiseConfig(GameManagerConfig config)
        {
            _config = config.CameraManagerConfig;

            _cinemachineBrain = Object.Instantiate(_config.CinemachineBrainPrefab);

            Object.DontDestroyOnLoad(_cinemachineBrain.gameObject);

            base.InitialiseConfig(config);
        }

        public override void Tick()
        {
            _blendingMode?.Tick();
            _currentMode?.Tick();
        }

        public async Task SwitchModeAsync(ICameraMode mode)
        {
            ICinemachineCamera previous = _cinemachineBrain.ActiveVirtualCamera;

            _blendingMode = _currentMode;

            _currentMode = mode;
            _currentMode.Enter();

            while (_cinemachineBrain.ActiveVirtualCamera == previous)
            {
                await Task.Yield();
            }

            while (_cinemachineBrain.IsBlending) 
            {
                await Task.Yield();
            }

            _blendingMode?.Exit();
            _blendingMode = null;
        }
    }
}