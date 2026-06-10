using System.Collections.Generic;
using UnityEngine;

namespace NoMoreFishAndChips.Rarities
{
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public interface IRarityManagerListener
    { }

    public class RarityManager : GameSystem<IRarityManagerListener>
    {
        private RarityManagerConfig _config;

        private Dictionary<Rarity, Color> _rarityColorMap = new();

        public override void Initialise(GameManagerConfig config)
        {
            _config = config.RarityManagerConfig;

            foreach (RarityColorMapping mapping in _config.RarityColorMappings)
            {
                _rarityColorMap.Add(mapping.Rarity, mapping.Color);
            }

            base.Initialise(config);
        }

        public Color GetColor(Rarity rarity)
        {
            return _rarityColorMap[rarity];
        }
    }
}