using UnityEngine;

namespace NoMoreFishAndChips.States
{
    [CreateAssetMenu(fileName = "DockStateConfig", menuName = "Configs/Managers/State/Gameplay/Intermission/DockStateConfig")]
    public class DockStateConfig : ScriptableObject
    {
        [SerializeField] private LayerMask _voyageResultsMask;
        [SerializeField] private float _startDuration = 3f;

        public LayerMask VoyageResultsMask => _voyageResultsMask;
        public float StartDuration => _startDuration;
    }
}