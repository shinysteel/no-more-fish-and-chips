using UnityEngine;

namespace NoMoreFishAndChips.Effects
{
    [CreateAssetMenu(fileName = "EffectManagerConfig", menuName = "Configs/Managers/EffectManagerConfig")]
    public class EffectManagerConfig : ScriptableObject
    {
        [SerializeField] private VFXScanner _vfxScanner;

        public VFXScanner VfxScanner => _vfxScanner;
    }
}