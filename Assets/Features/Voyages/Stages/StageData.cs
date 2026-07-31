using UnityEngine;
using NoMoreFishAndChips.States;

namespace NoMoreFishAndChips.Voyages
{
    [CreateAssetMenu(fileName = "StageData", menuName = "Data/Voyages/StageData")]
    public class StageData : ScriptableObject
    {
        [SerializeField] private StageId _id;
        [SerializeField] private Wave[] _waves;

        public StageId Id => _id;
        public Wave[] Waves => _waves;
    }
}