using UnityEngine;

namespace NoMoreFishAndChips.States
{
    [CreateAssetMenu(fileName = "DockStateConfig", menuName = "Configs/Managers/States/Gameplay/Intermission/DockStateConfig")]
    public class DockStateConfig : ScriptableObject
    {
        [SerializeField] private LayerMask _voyageResultsMask;
        [SerializeField] private float _startDelay = 1f;

        public LayerMask VoyageResultsMask => _voyageResultsMask;
        public float StartDelay => _startDelay;
    }
}