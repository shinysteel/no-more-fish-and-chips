using FishFlingers.Environments;
using UnityEngine;
using FishFlingers.Entities;
using FishFlingers.Effects;

[CreateAssetMenu(fileName = "GameplayStateConfig", menuName = "Configs/Managers/State/GameplayStateConfig")]
public class GameplayStateConfig : ScriptableObject
{
    [SerializeField] private Raft _raftPrefab;
    [SerializeField] private WaveSpawner _waveSpawnerPrefab;
    [SerializeField] private SalvageSpawner _salvageSpawnerPrefab;
    [SerializeField] private TileMarker _tileMarkerPrefab;
    [SerializeField] private GameplayEnvironment _gameplayEnvironmentPrefab;

    public Raft RaftPrefab => _raftPrefab;
    public WaveSpawner WaveSpawnerPrefab => _waveSpawnerPrefab;
    public SalvageSpawner SalvageSpawnerPrefab => _salvageSpawnerPrefab;
    public TileMarker TileMarkerPrefab => _tileMarkerPrefab;
    public GameplayEnvironment GameplayEnvironmentPrefab => _gameplayEnvironmentPrefab;
}
