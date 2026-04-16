using UnityEngine;
using System.Threading.Tasks;
using PrimeTween;

namespace FishFlingers.Entities
{
    public class RaftPlayerAttackLogic
    {
        private RaftPlayer _player;

        private bool _isAttacking;

        public bool IsAttacking => _isAttacking;
        
        public RaftPlayerAttackLogic(RaftPlayer player)
        {
            _player = player;
        }

        public async Task AttackAsync()
        {
            if (_isAttacking)
            {
                return;
            }

            _isAttacking = true;

            await _player.AnimateLogic.AttackAsync();

            _isAttacking = false;
        }
    }
}