using NoMoreFishAndChips.Environments;
using UnityEngine;
using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Effects;
using NoMoreFishAndChips.States;

[CreateAssetMenu(fileName = "GameplayStateConfig", menuName = "Configs/Managers/State/Gameplay/GameplayStateConfig")]
public class GameplayStateConfig : ScriptableObject
{
    [SerializeField] private IntermissionStateConfig _intermissionStateConfig;
    [SerializeField] private StageStateConfig _stageStateConfig;
    [SerializeField] private Raft _raftPrefab;
    [SerializeField] private VoyageRunner _voyageRunnerPrefab;
    [SerializeField] private DrowningSpawner _drowningSpawnerPrefab;
    [SerializeField] private SalvageSpawner _salvageSpawnerPrefab;
    [SerializeField] private EnvironmentMarker _environmentMarkerPrefab;
    [SerializeField] private GameplayEnvironment _gameplayEnvironmentPrefab;

    public IntermissionStateConfig IntermissionStageConfig => _intermissionStateConfig;
    public StageStateConfig StageStateConfig => _stageStateConfig;
    public Raft RaftPrefab => _raftPrefab;
    public VoyageRunner VoyageRunnerPrefab => _voyageRunnerPrefab;
    public DrowningSpawner DrowningSpawnerPrefab => _drowningSpawnerPrefab;
    public SalvageSpawner SalvageSpawnerPrefab => _salvageSpawnerPrefab;
    public EnvironmentMarker EnvironmentMarkerPrefab => _environmentMarkerPrefab;
    public GameplayEnvironment GameplayEnvironmentPrefab => _gameplayEnvironmentPrefab;
}
