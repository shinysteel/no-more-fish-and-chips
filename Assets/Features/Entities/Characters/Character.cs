using PrimeTween;
using PurrNet;
using UnityEngine;

namespace FishFlingers.Entities
{
    public abstract class Character : NetEntity
    {
        public CharacterData CharacterData => (CharacterData)_entityData;
        public CharacterModel CharacterModel => (CharacterModel)_entityModel;

        [SerializeField] protected Collider _characterCollider;
        public Collider CharacterCollider => _characterCollider;

        private CharacterRagdollLogic _ragdollLogic;
        protected CharacterPhysicsLogic _physicsLogic;
        protected CharacterDefeatLogic _defeatLogic;
        protected CharacterStunLogic _stunLogic;

        public CharacterRagdollLogic RagdollLogic => _ragdollLogic;
        public CharacterPhysicsLogic PhysicsLogic => _physicsLogic;

        protected override void OnSpawned()
        {
            _ragdollLogic = new CharacterRagdollLogic(this);

            // Some characters setup their own inherited logic script
            _physicsLogic ??= new CharacterPhysicsLogic(this);

            _defeatLogic = new CharacterDefeatLogic(this);

            _stunLogic = new CharacterStunLogic();

            _healthModule.OnChanged += HandleHealthChanged;

            base.OnSpawned();
        }

        protected override void OnDespawned()
        {
            _healthModule.OnChanged -= HandleHealthChanged;

            base.OnDespawned();
        }

        private void HandleHealthChanged(int previous, int current)
        {
            if (current == 0)
            {
                return;
            }

            if (current < previous)
            {
                CharacterModel.AnimateHurt();
            }
        }

        protected virtual void Update()
        {
            if (!isFullySpawned)
            {
                return;
            }

            if (!isOwner)
            {
                return;
            }
            
            _defeatLogic.Tick();
            _physicsLogic.Tick();
            _stunLogic.Tick();
        }

        protected virtual void FixedUpdate()
        {
            if (!isFullySpawned)
            {
                return;
            }

            if (!isOwner)
            {
                return;
            }

            _physicsLogic.FixedTick();
        }

        [ServerRpc]
        public void StunRpc(float duration)
        {
            _stunLogic.Stun(duration);
        }
    }

    public abstract class Character<T> : Character where T : CharacterData
    {
        public T Data => (T)_entityData;
    }
}