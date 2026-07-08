using NoMoreFishAndChips.Environments;
using UnityEngine;

namespace NoMoreFishAndChips.States
{
    [CreateAssetMenu(fileName = "IntermissionStateConfig", menuName = "Configs/Managers/State/Gameplay/Intermission/IntermissionStateConfig")]
    public class IntermissionStateConfig : ScriptableObject
    {
        [SerializeField] private DockStateConfig _dockStateConfig;
        [SerializeField] private DepartStateConfig _departStateConfig;
        [SerializeField] private Island _islandPrefab;
        [SerializeField] private float _islandOffset = 6f;

        public DockStateConfig DockStateConfig => _dockStateConfig;
        public DepartStateConfig DepartStateConfig => _departStateConfig;
        public Island IslandPrefab => _islandPrefab;
        public float IslandOffset => _islandOffset;
    }
}