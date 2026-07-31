using UnityEngine;

namespace NoMoreFishAndChips.Voyages
{
    [CreateAssetMenu(fileName = "VoyageManagerConfig", menuName = "Configs/Managers/VoyageManagerConfig")]
    public class VoyageManagerConfig : ScriptableObject
    {
        [SerializeField] private StageDataScanner _stageDataScanner;

        public StageDataScanner StageDataScanner => _stageDataScanner;
    }
}