using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "EntityEffectsSettings", menuName = "Settings/Entities/EntityEffectsSettings")]
    public class EntityEffectsSettings : ScriptableObject
    {
        [SerializeField] private bool _alwaysAnimateHurt = false;

        public bool AlwaysAnimateHurt => _alwaysAnimateHurt;
    }
}