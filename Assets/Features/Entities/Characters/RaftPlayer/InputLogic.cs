using UnityEngine;

namespace FishFlingers.Entities
{
    public class InputLogic
    {
        private RaftPlayer _player;

        // Vars always active
        private bool _click;
        private Vector2 _rawMouse;

        public bool Click => _click;
        public Vector2 RawMouse => _rawMouse;

        // Vars dependent on RaftPlayer.CanAct
        private Vector2 _gameplayMouse;
        private Vector3 _moveDirection;
        private bool _jump;
        private bool _ascend;
        private bool _interact;

        public Vector2 GameplayMouse => _gameplayMouse;
        public Vector3 MoveDirection => _moveDirection;
        public bool Jump => _jump;
        public bool Ascend => _ascend;
        public bool Interact => _interact;

        public InputLogic(RaftPlayer player)
        {
            _player = player;
        }

        public void Tick()
        {
            _click = Input.GetMouseButtonDown(0);
            _rawMouse = Input.mousePosition;

            if (_player.CanAct)
            {
                _gameplayMouse = _rawMouse;
                _moveDirection = Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")), 1f);
                _jump = Input.GetKeyDown(KeyCode.Space);
                _ascend = Input.GetKey(KeyCode.Space);
                _interact = Input.GetKeyDown(KeyCode.F);
            }
            else
            {
                _moveDirection = Vector3.zero;
                _jump = false;
                _ascend = false;
                _interact = false;
            }
        }
    }
}