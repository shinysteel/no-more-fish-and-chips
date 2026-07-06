using NoMoreFishAndChips.Environments;
using UnityEngine;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Effects;

[CreateAssetMenu(fileName = "GameplayStateConfig", menuName = "Configs/Managers/State/GameplayStateConfig")]
public class GameplayStateConfig : ScriptableObject
{
    [SerializeField] private Raft _raftPrefab;
    [SerializeField] private WaveSpawner _waveSpawnerPrefab;
    [SerializeField] private DrowningSpawner _drowningSpawnerPrefab;
    [SerializeField] private SalvageSpawner _salvageSpawnerPrefab;
    [SerializeField] private EnvironmentMarker _environmentMarkerPrefab;
    [SerializeField] private GameplayEnvironment _gameplayEnvironmentPrefab;

    public Raft RaftPrefab => _raftPrefab;
    public WaveSpawner WaveSpawnerPrefab => _waveSpawnerPrefab;
    public DrowningSpawner DrowningSpawnerPrefab => _drowningSpawnerPrefab;
    public SalvageSpawner SalvageSpawnerPrefab => _salvageSpawnerPrefab;
    public EnvironmentMarker EnvironmentMarkerPrefab => _environmentMarkerPrefab;
    public GameplayEnvironment GameplayEnvironmentPrefab => _gameplayEnvironmentPrefab;
}
