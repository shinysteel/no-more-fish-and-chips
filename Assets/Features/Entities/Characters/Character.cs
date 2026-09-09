using NoMoreFishAndChips.States;
using PrimeTween;
using PurrNet;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class Character : Entity
    {
        public CharacterModel CharacterModel => (CharacterModel)_entityModel;

        public CharacterDefinitionData CharacterDefinitionData => (CharacterDefinitionData)_entityDefinitionData;

        public CharacterPhysicsLogic CharacterPhysicsLogic => (CharacterPhysicsLogic)EntityPhysicsLogic;
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

        [TargetRpc]
        public void ChangePoiseRpc(PlayerID id, float change)
        {
            CharacterActLogic.ChangePoise(change);
        }
    }

    public abstract class Character<T> : Character where T : EntityDefinitionData
    {
        public T DefinitionData => (T)_entityDefinitionData;
    }
}