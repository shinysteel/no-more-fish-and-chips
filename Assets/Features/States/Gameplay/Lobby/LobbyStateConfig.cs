using NoMoreFishAndChips.Environments;
using UnityEngine;

namespace NoMoreFishAndChips.States
{
    [CreateAssetMenu(fileName = "LobbyStateConfig", menuName = "Configs/Managers/State/Gameplay/LobbyStateConfig")]
    public class LobbyStateConfig : ScriptableObject
    {
        [SerializeField] private Island _islandPrefab;

        public Island IslandPrefab => _islandPrefab;
    }
}