using NoMoreFishAndChips.States;
using PrimeTween;
using PurrNet;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class Character : Entity
    {
        public CharacterModel CharacterModel => (CharacterModel)_entityModel;

        public CharacterPhysicsLogic CharacterPhysicsModule => (CharacterPhysicsLogic)EntityPhysicsLogic;
        public CharacterRagdollLogic CharacterRagdollLogic => GetLogic<CharacterRagdollLogic>();
        public CharacterActLogic CharacterActLogic => GetLogic<CharacterActLogic>();

        protected override EntityLogicFactory CreateLogicFactory()
        {
            return new CharacterLogicFactory();
        }

        protected override void OnInitializeModules()
        {
            base.OnInitializeModules();

            CharacterLogicFactory factory = (CharacterLogicFactory)_logicFactory;

            AddLogic(typeof(CharacterRagdollLogic), factory.CreateRagdollLogic(this));
            AddLogic(typeof(CharacterActLogic), factory.CreateActLogic(this));
        }

        [ServerRpc]
        public void StunRpc(float duration)
        {
            CharacterActLogic.Stun(duration);
        }
    }

    public abstract class Character<T> : Character where T : EntityDefinitionData
    {
        public T DefinitionData => (T)_entityDefinitionData;
    }
}