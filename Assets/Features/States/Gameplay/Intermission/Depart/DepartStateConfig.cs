using UnityEngine;

namespace NoMoreFishAndChips.States
{
    [CreateAssetMenu(fileName = "DepartStateConfig", menuName = "Configs/Managers/States/Gameplay/Intermission/DepartStateConfig")]
    public class DepartStateConfig : ScriptableObject
    {
        [SerializeField] private float _departDelay = 5f;

        public float DepartDelay => _departDelay;
    }
}