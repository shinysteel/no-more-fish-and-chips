using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class CharacterActLogic : CharacterLogic
    {
        private bool _isStunned;
        private float _stunTimer;

        public virtual bool CanAct => !_character.EntityDefeatLogic.IsDefeated && !_isStunned;

        public CharacterActLogic(Character character) : base(character)
        { }

        public override void Tick()
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