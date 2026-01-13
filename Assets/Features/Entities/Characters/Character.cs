using UnityEngine;

namespace FishFlingers.Entities
{
    public abstract class Character : NetEntity
    {
        public CharacterData CharacterData => (CharacterData)_entityData;
    }

    public abstract class Character<T> : Character where T : CharacterData
    {
        public T Data => (T)_entityData;
    }
}