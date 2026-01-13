using UnityEngine;

namespace FishFlingers.Entities
{
    public abstract class Structure : NetEntity
    {
        public StructureData StructureData => (StructureData)_entityData;
    }

    public abstract class Structure<T> : Structure where T : StructureData
    {
        public T Data => (T)_entityData;
    }
}