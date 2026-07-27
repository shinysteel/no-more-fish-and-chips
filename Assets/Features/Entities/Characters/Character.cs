using PrimeTween;
using PurrNet;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class Character : Entity
    {
        public CharacterModel CharacterModel => (CharacterModel)_entityModel;
        public CharacterPhysicsModule CharacterPhysicsModule => (CharacterPhysicsModule)_entityPhysicsModule;

        private CharacterRagdollLogic _ragdollLogic;
        protected CharacterActLogic _characterActLogic;

        public CharacterRagdollLogic RagdollLogic => _ragdollLogic;
        public CharacterActLogic CharacterActLogic => _characterActLogic;

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

            _characterActLogic = CreateActLogic();

            base.OnSpawned();
        }

        protected virtual CharacterActLogic CreateActLogic()
        {
            return new CharacterActLogic(this);
        }

        protected override void Update()
        {
            base.Update();

            if (!isFullySpawned)
            {
                return;
            }
            
            _characterActLogic.Tick();
        }

        [ServerRpc]
        public void StunRpc(float duration)
        {
            _characterActLogic.Stun(duration);
        }
    }

    public abstract class Character<T> : Character where T : EntityDefinitionData
    {
        public T DefinitionData => (T)_entityDefinitionData;
    }
}