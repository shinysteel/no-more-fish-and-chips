using NoMoreFishAndChips.Inventories;
using UnityEngine;

namespace NoMoreFishAndChips.Items
{
    [CreateAssetMenu(fileName = "ItemManagerConfig", menuName = "Configs/Managers/ItemManagerConfig")]
    public class ItemManagerConfig : ScriptableObject
    {
        [SerializeField] private ItemDataScanner _itemDataScanner;
        [SerializeField] private Sprite[] _assignmentSprites;

        public ItemDataScanner ItemDataScanner => _itemDataScanner;
        public Sprite[] AssignmentSprites => _assignmentSprites;
    }
}