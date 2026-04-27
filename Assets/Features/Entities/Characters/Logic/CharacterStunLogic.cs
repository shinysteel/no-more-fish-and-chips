using UnityEngine;

namespace FishFlingers.Entities
{
    public class CharacterStunLogic
    {
        private bool _isStunned;
        public bool IsStunned => _isStunned;

        private float _stunTimer;

        public void Tick()
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