using UnityEngine;

namespace NoMoreFishAndChips.States
{
    [CreateAssetMenu(fileName = "ArriveStateConfig", menuName = "Configs/Managers/State/Gameplay/Intermission/ArriveStateConfig")]
    public class ArriveStateConfig : ScriptableObject
    {
        [SerializeField] private float _arriveDelay = 5f;

        public float ArriveDelay => _arriveDelay;
    }
}