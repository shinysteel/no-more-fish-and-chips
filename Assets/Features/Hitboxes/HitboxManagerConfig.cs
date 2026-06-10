using UnityEngine;

namespace NoMoreFishAndChips.Hitboxes
{
    [CreateAssetMenu(fileName = "HitboxManagerConfig", menuName = "Configs/Managers/HitboxManagerConfig")]
    public class HitboxManagerConfig : ScriptableObject
    {
        [SerializeField] private bool _drawGizmos;

        public bool DrawGizmos => _drawGizmos;
    }
}