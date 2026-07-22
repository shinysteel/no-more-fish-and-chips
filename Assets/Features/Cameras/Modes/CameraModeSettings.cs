using Unity.Cinemachine;
using UnityEngine;

namespace NoMoreFishAndChips.Cameras
{
    public abstract class CameraModeSettings : ScriptableObject
    {
        [SerializeField] private CinemachineCamera _cinemachineCameraPrefab;

        public CinemachineCamera CinemachineCameraPrefab => _cinemachineCameraPrefab;
    }
}