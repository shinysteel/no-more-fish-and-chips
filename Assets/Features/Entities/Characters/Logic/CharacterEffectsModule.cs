using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class CharacterEffectsModule : EntityEffectsModule
    {
        private Character _character;

        public CharacterEffectsModule(Character character) : base(character)
        {
            _character = character;
        }

        public override void AnimateHurt()
        {
            _character.CharacterModel.FlashRed();

            // The animator is already networked, and so only the owner needs to do this
            if (_character.isOwner)
            {
                _character.CharacterModel.AdditiveHurt();
            }
        }
    }
}