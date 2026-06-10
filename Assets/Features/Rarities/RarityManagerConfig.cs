using System;
using UnityEngine;

namespace NoMoreFishAndChips.Rarities
{
    [Serializable]
    public class RarityColorMapping
    {
        [SerializeField] private Rarity _rarity;
        [SerializeField] private Color _color;

        public Rarity Rarity => _rarity;
        public Color Color => _color;
    }

    [CreateAssetMenu(fileName = "RarityManagerConfig", menuName = "Configs/Managers/RarityManagerConfig")]
    public class RarityManagerConfig : ScriptableObject
    {
        [SerializeField] private RarityColorMapping[] _rarityColorMappings;

        public RarityColorMapping[] RarityColorMappings => _rarityColorMappings;
    }
}