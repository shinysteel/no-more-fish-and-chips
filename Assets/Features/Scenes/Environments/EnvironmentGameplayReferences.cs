using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class EnvironmentGameplayReferences : MonoBehaviour
    {
        [SerializeField] private Ocean _ocean;

        public Ocean Ocean => _ocean;
    }
}