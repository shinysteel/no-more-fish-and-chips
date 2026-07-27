using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class CharacterLogic : EntityLogic
    {
        protected Character _character;

        public CharacterLogic(Character character) : base(character)
        {
            _character = character;
        }
    }
}