using FishFlingers.Environments;
using UnityEngine;
using FishFlingers.Entities;

[CreateAssetMenu(fileName = "GameplayStateConfig", menuName = "Configs/Managers/State/GameplayStateConfig")]
public class GameplayStateConfig : ScriptableObject
{
    [SerializeField] private Raft _raftPrefab;
    [SerializeField] private WaveSpawner _waveSpawnerPrefab;
    [SerializeField] private SalvageSpawner _salvageSpawnerPrefab;
    [SerializeField] private GameplayEnvironment _gameplayEnvironmentPrefab;

    public Raft RaftPrefab => _raftPrefab;
    public WaveSpawner WaveSpawnerPrefab => _waveSpawnerPrefab;
    public SalvageSpawner SalvageSpawnerPrefab => _salvageSpawnerPrefab;
    public GameplayEnvironment GameplayEnvironmentPrefab => _gameplayEnvironmentPrefab;
}
