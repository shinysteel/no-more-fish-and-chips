using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    [CreateAssetMenu(fileName = "EnvironmentManagerConfig", menuName = "Configs/Managers/EnvironmentManagerConfig")]
    public class EnvironmentManagerConfig : ScriptableObject
    {
        [SerializeField] private PropScanner _propScanner;

        public PropScanner PropScanner => _propScanner;
    }
}