using UnityEngine;

namespace NoMoreFishAndChips.States
{
    [CreateAssetMenu(fileName = "StageData", menuName = "Data/States/Gameplay/StageData")]
    public class StageData : ScriptableObject
    {
        [SerializeField] private Wave[] _waves;

        public Wave[] Waves => _waves;
    }
}