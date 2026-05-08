using PrimeTween;
using PurrNet;
using UnityEngine;

namespace FishFlingers.Entities
{
    public abstract class Character : NetEntity
    {
        public CharacterDefinitionData CharacterDefinitionData => (CharacterDefinitionData)_entityDefinitionData;
        public CharacterModel CharacterModel => (CharacterModel)_entityModel;
        public CharacterPhysicsModule CharacterPhysicsModule => (CharacterPhysicsModule)_entityPhysicsModule;

        [SerializeField] protected Collider _characterCollider;
        public Collider CharacterCollider => _characterCollider;

        private CharacterRagdollLogic _ragdollLogic;
        protected CharacterStunLogic _stunLogic;

        public CharacterRagdollLogic RagdollLogic => _ragdollLogic;

        protected override EntityDefeatModule CreateDefeatModule()
        {
            return new CharacterDefeatModule(this);
        }

        protected override EntityEffectsModule CreateEffectsModule()
        {
            return new CharacterEffectsModule(this);
        }

        protected override EntityPhysicsModule CreatePhysicsModule()
        {
            return new CharacterPhysicsModule(this, _rigidbody);
        }

        protected override void OnSpawned()
        {
            _ragdollLogic = new CharacterRagdollLogic(this);

            _stunLogic = new CharacterStunLogic(this);

            base.OnSpawned();
        }

        protected override void Update()
        {
            base.Update();

            if (!isFullySpawned)
            {
                return;
            }
            
            _stunLogic.Tick();
        }

        [ServerRpc]
        public void StunRpc(float duration)
        {
            _stunLogic.Stun(duration);
        }
    }

    public abstract class Character<T> : Character where T : CharacterDefinitionData
    {
        public T DefinitionData => (T)_entityDefinitionData;
    }
}