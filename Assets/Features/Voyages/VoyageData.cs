using UnityEngine;

namespace NoMoreFishAndChips.Voyages
{
    [CreateAssetMenu(fileName = "VoyageData", menuName = "Data/Voyage/VoyageData")]
    public class VoyageData : ScriptableObject
    {
        [SerializeField] private VoyageId _id;
        [SerializeField] private StageData[] _stageDatas;

        public VoyageId Id => _id;
        public StageData[] StageDatas => _stageDatas;
    }
}