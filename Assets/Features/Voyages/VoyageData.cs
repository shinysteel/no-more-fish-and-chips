using UnityEngine;

namespace NoMoreFishAndChips.Voyages
{
    [CreateAssetMenu(fileName = "VoyageData", menuName = "Data/Voyage/VoyageData")]
    public class VoyageData : ScriptableObject
    {
        [SerializeField] private StageData[] _stageDatas;

        public StageData[] StageDatas => _stageDatas;
    }
}