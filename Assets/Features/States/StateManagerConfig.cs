using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.States
{
    [CreateAssetMenu(fileName = "StateManagerConfig", menuName = "Configs/Managers/State/StateManagerConfig")]
    public class StateManagerConfig : ScriptableObject
    {
        [SerializeField] private MenusStateConfig _menusStateConfig;
        [SerializeField] private GameplayStateConfig _gameplayStateConfig;
        [SerializeField] private StateSynchroniser _stateSynchroniserPrefab;

        public MenusStateConfig MenusStateConfig => _menusStateConfig;
        public GameplayStateConfig GameplayStateConfig => _gameplayStateConfig;
        public StateSynchroniser StateSynchroniserPrefab => _stateSynchroniserPrefab;
    }
}