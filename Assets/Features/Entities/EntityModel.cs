using FishFlingers.States;
using PurrNet;
using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class EntityModel : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private NetworkAnimator _networkAnimator;

        private GameplayContext _context;

        public Animator Animator => _animator;

        public void Initialise(GameplayContext context)
        {
            _context = context;
        }

        public void SetTrigger(string name)
        {
            if (_networkAnimator == null)
            {
                _animator.SetTrigger(name);
            }
            else
            {
                _networkAnimator.SetTrigger(name);
            }
        }
    }
}