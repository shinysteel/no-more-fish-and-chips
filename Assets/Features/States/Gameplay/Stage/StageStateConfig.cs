using NoMoreFishAndChips.Environments;
using UnityEngine;

namespace NoMoreFishAndChips.States
{
    [CreateAssetMenu(fileName = "StageStateConfig", menuName = "Configs/Managers/State/Gameplay/StageStateConfig")]
    public class StageStateConfig : ScriptableObject
    {
        [SerializeField] private StageData _clamClusterStageData;
        [SerializeField] private StageData _sharkDenStageData;
        [SerializeField] private StageData _squidStrongholdStageData;

        public StageData ClamClusterStageData => _clamClusterStageData;
        public StageData SharkDenStageData => _sharkDenStageData;
        public StageData SquidStrongholdStageData => _squidStrongholdStageData;
    }
}