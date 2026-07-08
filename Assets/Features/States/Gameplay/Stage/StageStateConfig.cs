using NoMoreFishAndChips.Environments;
using UnityEngine;

namespace NoMoreFishAndChips.States
{
    [CreateAssetMenu(fileName = "StageStateConfig", menuName = "Configs/Managers/State/Gameplay/StageStateConfig")]
    public class StageStateConfig : ScriptableObject
    {
        [SerializeField] private StageData _defaultStageData;

        public StageData DefaultStageData => _defaultStageData;
    }
}