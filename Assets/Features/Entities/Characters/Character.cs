using NoMoreFishAndChips.Hitboxes;
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

        public override void HitRpc(PlayerID id, Hit hit, Vector3 direction)
        {
            base.HitRpc(id, hit, direction);

            if (isSpawned)
            {
                CharacterActLogic.ChangePoise(-hit.PoiseDamage);
            }
        }
    }

    public abstract class Character<T> : Character where T : EntityDefinitionData
    {
        public T DefinitionData => (T)_entityDefinitionData;
    }
}