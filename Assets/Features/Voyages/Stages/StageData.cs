using UnityEngine;
using NoMoreFishAndChips.States;

namespace NoMoreFishAndChips.Voyages
{
    [CreateAssetMenu(fileName = "StageData", menuName = "Data/Voyages/StageData")]
    public class StageData : ScriptableObject
    {
        [SerializeField] private Wave[] _waves;

        public Wave[] Waves => _waves;
    }
}