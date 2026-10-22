using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class EnvironmentGameplayReferences : MonoBehaviour
    {
        [SerializeField] private Light _light;
        [SerializeField] private Sky _sky;

        public Light Light => _light;
        public Sky Sky => _sky;
    }
}