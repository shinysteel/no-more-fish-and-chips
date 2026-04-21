using UnityEngine;

namespace FishFlingers.Entities
{
    public abstract class Character : NetEntity
    {
        public CharacterData CharacterData => (CharacterData)_entityData;
        public CharacterModel CharacterModel => (CharacterModel)_entityModel;

        protected CharacterDefeatLogic _defeatLogic;
        private CharacterPhysicsLogic _physicsLogic;
        private CharacterRagdollLogic _ragdollLogic;

        public CharacterRagdollLogic RagdollLogic => _ragdollLogic;

        protected override void OnSpawned()
        {
            _defeatLogic = new CharacterDefeatLogic(this);
            _physicsLogic = new CharacterPhysicsLogic(this);
            _ragdollLogic = new CharacterRagdollLogic(this);

            base.OnSpawned();
        }
    }

    public abstract class Character<T> : Character where T : CharacterData
    {
        public T Data => (T)_entityData;
    }
}