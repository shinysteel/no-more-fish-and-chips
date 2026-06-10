using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "RaftPlayerDefeatSettings", menuName = "Settings/Entities/RaftPlayerDefeatSettings")]
    public class RaftPlayerDefeatSettings : CharacterDefeatSettings
    {
        [SerializeField] private float _moveInterval = 0.75f;
        [SerializeField] private float _moveLinearStrength = 10f;
        [SerializeField] private float _moveAngularStrength = -10f;
        [SerializeField] private float _movePitch = -20f;
        [SerializeField] private float _stabalisationStrength = 20f;
        [SerializeField] private float _stablisationDamping = 2.5f;
        [SerializeField] private float _reviveRadius = 3f;
        [SerializeField] private float _reviveStrength = 50f;
        [SerializeField] private LayerMask _reviveMask;

        public float MoveInterval => _moveInterval;
        public float MoveLinearStrength => _moveLinearStrength;
        public float MoveAngularStrength => _moveAngularStrength;
        public float MovePitch => _movePitch;
        public float StabalisationStrength => _stabalisationStrength;
        public float StabalisationDamping => _stablisationDamping;
        public float ReviveRadius => _reviveRadius;
        public LayerMask ReviveMask => _reviveMask;
        public float ReviveStrength => _reviveStrength;
    }
}