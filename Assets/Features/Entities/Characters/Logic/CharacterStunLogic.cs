using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class CharacterStunLogic
    {
        private Character _character;

        private bool _isStunned;
        public bool IsStunned => _isStunned;

        private float _stunTimer;

        public CharacterStunLogic(Character character)
        {
            _character = character;
        }

        public void Tick()
        {
            if (!_character.isOwner)
            {
                return;
            }

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