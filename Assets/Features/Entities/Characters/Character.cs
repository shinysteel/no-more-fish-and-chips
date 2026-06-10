using PrimeTween;
using PurrNet;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class Character : NetEntity
    {
        public CharacterModel CharacterModel => (CharacterModel)_entityModel;
        public CharacterDefeatModule CharacterDefeatModule => (CharacterDefeatModule)_entityDefeatModule;
        public CharacterPhysicsModule CharacterPhysicsModule => (CharacterPhysicsModule)_entityPhysicsModule;

        private CharacterRagdollLogic _ragdollLogic;
        protected CharacterStunLogic _stunLogic;

        public CharacterRagdollLogic RagdollLogic => _ragdollLogic;

        protected override EntityDefeatModule CreateDefeatModule()
        {
            return new CharacterDefeatModule(this, GetNetIsDefeated, SetNetIsDefeated);
        }

        protected override EntityEffectsModule CreateEffectsModule()
        {
            return new CharacterEffectsModule(this);
        }

        protected override EntityPhysicsModule CreatePhysicsModule()
        {
            return new CharacterPhysicsModule(this, _rigidbody, _collider);
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

    public abstract class Character<T> : Character where T : EntityDefinitionData
    {
        public T DefinitionData => (T)_entityDefinitionData;
    }
}