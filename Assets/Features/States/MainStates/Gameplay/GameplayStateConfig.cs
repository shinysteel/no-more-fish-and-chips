using FishFlingers.Environments;
using UnityEngine;
using FishFlingers.Entities;

[CreateAssetMenu(fileName = "GameplayStateConfig", menuName = "Configs/Managers/State/GameplayStateConfig")]
public class GameplayStateConfig : ScriptableObject
{
    [SerializeField] private RaftPlayer _raftPlayerPrefab;
    [SerializeField] private Raft _raftPrefab;
    [SerializeField] private WaveSpawner _waveSpawnerPrefab;
    [SerializeField] private SalvageSpawner _salvageSpawnerPrefab;

    public RaftPlayer RaftPlayerPrefab => _raftPlayerPrefab;
    public Raft RaftPrefab => _raftPrefab;
    public WaveSpawner WaveSpawnerPrefab => _waveSpawnerPrefab;
    public SalvageSpawner SalvageSpawnerPrefab => _salvageSpawnerPrefab;
}
