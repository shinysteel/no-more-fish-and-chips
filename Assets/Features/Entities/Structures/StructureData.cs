using FishFlingers.Items;
using UnityEngine;

namespace FishFlingers.Entities
{
    [CreateAssetMenu(fileName = "StructureData", menuName = "Data/Entities/Structures/StructureData")]
    public abstract class StructureData : EntityData
    {
        [SerializeField] private Recipe _recipe;

        public Recipe Recipe => _recipe;
    }
}