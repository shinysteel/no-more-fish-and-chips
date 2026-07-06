using NoMoreFishAndChips.Environments;
using UnityEngine;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Effects;
using NoMoreFishAndChips.States;

[CreateAssetMenu(fileName = "GameplayStateConfig", menuName = "Configs/Managers/State/GameplayStateConfig")]
public class GameplayStateConfig : ScriptableObject
{
    [SerializeField] private LobbyStateConfig _lobbyStateConfig;
    [SerializeField] private StageStateConfig _stageStateConfig;
    [SerializeField] private IntermissionStateConfig _intermissionStateConfig;
    [SerializeField] private Raft _raftPrefab;
    [SerializeField] private WaveSpawner _waveSpawnerPrefab;
    [SerializeField] private DrowningSpawner _drowningSpawnerPrefab;
    [SerializeField] private SalvageSpawner _salvageSpawnerPrefab;
    [SerializeField] private EnvironmentMarker _environmentMarkerPrefab;
    [SerializeField] private GameplayEnvironment _gameplayEnvironmentPrefab;

    public LobbyStateConfig LobbyStateConfig => _lobbyStateConfig;
    public StageStateConfig StageStateConfig => _stageStateConfig;
    public IntermissionStateConfig IntermissionStateConfig => _intermissionStateConfig;
    public Raft RaftPrefab => _raftPrefab;
    public WaveSpawner WaveSpawnerPrefab => _waveSpawnerPrefab;
    public DrowningSpawner DrowningSpawnerPrefab => _drowningSpawnerPrefab;
    public SalvageSpawner SalvageSpawnerPrefab => _salvageSpawnerPrefab;
    public EnvironmentMarker EnvironmentMarkerPrefab => _environmentMarkerPrefab;
    public GameplayEnvironment GameplayEnvironmentPrefab => _gameplayEnvironmentPrefab;
}
