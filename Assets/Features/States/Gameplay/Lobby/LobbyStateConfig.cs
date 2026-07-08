using NoMoreFishAndChips.Environments;
using UnityEngine;

namespace NoMoreFishAndChips.States
{
    [CreateAssetMenu(fileName = "LobbyStateConfig", menuName = "Configs/Managers/State/Gameplay/Lobby/LobbyStateConfig")]
    public class LobbyStateConfig : ScriptableObject
    {
        [SerializeField] private DockStateConfig _dockStateConfig;
        [SerializeField] private DepartStateConfig _departStateConfig;
        [SerializeField] private Island _islandPrefab;

        public DockStateConfig DockStateConfig => _dockStateConfig;
        public DepartStateConfig DepartStateConfig => _departStateConfig;
        public Island IslandPrefab => _islandPrefab;
    }
}