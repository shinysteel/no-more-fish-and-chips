using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Items
{
    [CreateAssetMenu(fileName = "ItemDropTable", menuName = "Tables/ItemDropTable")]
    public class DropTable : ScriptableObject
    {
        [SerializeField] private WeightedEntry<ItemId>[] _entries;

        public WeightedEntry<ItemId>[] Entries => _entries;
    }
}