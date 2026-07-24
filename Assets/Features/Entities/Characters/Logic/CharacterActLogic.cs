using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class CharacterActLogic
    {
        private Character _character;

        private bool _isStunned;
        private float _stunTimer;

        public virtual bool CanAct => !_character.EntityDefeatModule.IsDefeated && !_isStunned;

        public CharacterActLogic(Character character)
        {
            _character = character;
        }

        public void Tick()
        {
            if (!_character.isOwner)
            {
                return;
            }

            StunTick();
        }

        private void StunTick()
        {
            _stunTimer -= Time.deltaTime;
            _stunTimer = Mathf.Max(_stunTimer, 0f);

            _isStunned = _stunTimer > 0f;
        }

        public void Stun(float duration)
        {
            _stunTimer = Mathf.Max(_stunTimer, duration);
        }
    }
}