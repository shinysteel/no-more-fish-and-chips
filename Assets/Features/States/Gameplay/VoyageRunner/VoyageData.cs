using UnityEngine;

namespace NoMoreFishAndChips.States
{
    [CreateAssetMenu(fileName = "VoyageData", menuName = "Data/States/Gameplay/VoyageData")]
    public class VoyageData : ScriptableObject
    {
        [SerializeField] private StageData[] _stageDatas;

        public StageData[] StageDatas => _stageDatas;
    }
}