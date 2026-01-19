using UnityEngine;

namespace FishFlingers.Entities
{
    public class InputLogic
    {
        private RaftPlayer _player;

        private Vector2 _direction;
        private Vector2 _mouse;
        private bool _jump;
        private bool _ascend;
        private bool _interact;

        public Vector2 Direction => _direction;
        public Vector2 Mouse => _mouse;
        public bool Jump => _jump;
        public bool Ascend => _ascend;
        public bool Interact => _interact;

        public InputLogic(RaftPlayer player)
        {
            _player = player;
        }

        public void Tick()
        {
            if (_player.CanAct)
            {
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");

                _direction = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
                _mouse = Input.mousePosition;
                _jump = Input.GetKeyDown(KeyCode.Space);
                _ascend = Input.GetKey(KeyCode.Space);
                _interact = Input.GetKeyDown(KeyCode.F);
            }
            else
            {
                _direction = Vector2.zero;
                _jump = false;
                _ascend = false;
                _interact = false;
            }
        }
    }
}