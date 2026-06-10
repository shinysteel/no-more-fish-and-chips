using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Cameras
{
    [CreateAssetMenu(fileName = "CameraManagerConfig", menuName = "Configs/Managers/CameraManagerConfig")]
    public class CameraManagerConfig : ScriptableObject
    {
        [SerializeField] private Camera _mainCameraPrefab;

        public Camera MainCameraPrefab => _mainCameraPrefab;
    }
}