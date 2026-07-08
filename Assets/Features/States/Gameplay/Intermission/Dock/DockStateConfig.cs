using UnityEngine;

namespace NoMoreFishAndChips.States
{
    [CreateAssetMenu(fileName = "DockStateConfig", menuName = "Configs/Managers/State/Gameplay/Intermission/DockStateConfig")]
    public class DockStateConfig : ScriptableObject
    {
        [SerializeField] private float _startDuration = 3f;

        public float StartDuration => _startDuration;
    }
}