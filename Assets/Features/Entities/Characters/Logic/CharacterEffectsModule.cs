using UnityEngine;

namespace FishFlingers.Entities
{
    public class CharacterEffectsModule : EntityEffectsModule
    {
        public Character Character => (Character)_entity;

        public CharacterEffectsModule(Character character) : base(character)
        { }

        public override void AnimateHurt()
        {
            Character.CharacterModel.FlashRed();

            // The animator is already networked, and so only the owner needs to do this
            if (Character.IsOwner)
            {
                Character.CharacterModel.AdditiveHurt();
            }
        }
    }
}