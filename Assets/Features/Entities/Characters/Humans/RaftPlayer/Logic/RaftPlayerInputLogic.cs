using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class RaftPlayerInputLogic
    {
        private RaftPlayer _player;

        // Vars always active
        private Vector2 _mouse;
        private float _scroll;
        private bool _shift;
        private bool _leftClick;
        private bool _rightClick;
        private bool _one;
        private bool _two;
        private bool _three;
        private bool _rotateItem;
        private bool _dropItem;
        private bool _toggleSettings;
        private bool _toggleFishingBag;
        private bool _toggleCraftingKit;

        public Vector2 Mouse => _mouse;
        public float Scroll => _scroll;
        public bool Shift => _shift;
        public bool LeftClick => _leftClick;
        public bool RightClick => _rightClick;
        public bool One => _one;
        public bool Two => _two;
        public bool Three => _three;
        public bool RotateItem => _rotateItem;
        public bool DropItem => _dropItem;
        public bool ToggleSettings => _toggleSettings;
        public bool ToggleFishingBag => _toggleFishingBag;
        public bool ToggleCraftingKit => _toggleCraftingKit;
        
        // Vars dependent on RaftPlayer.CanAct
        private Vector2 _gameplayMouse;
        private Vector3 _moveDirection;
        private bool _jump;
        private bool _interact;

        public Vector2 GameplayMouse => _gameplayMouse;
        public Vector3 MoveDirection => _moveDirection;
        public bool Jump => _jump;
        public bool Interact => _interact;

        private const string MouseScrollWheelAxis = "Mouse ScrollWheel";
        private const string HorizontalAxis = "Horizontal";
        private const string VerticalAxis = "Vertical";

        public RaftPlayerInputLogic(RaftPlayer player)
        {
            _player = player;
        }

        public void Tick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            _mouse = Input.mousePosition;
            _scroll = Input.GetAxis(MouseScrollWheelAxis);
            _shift = Input.GetKey(KeyCode.LeftShift);
            _leftClick = Input.GetMouseButtonDown(0);
            _rightClick = Input.GetMouseButtonDown(1);
            _one = Input.GetKeyDown(KeyCode.Alpha1);
            _two = Input.GetKeyDown(KeyCode.Alpha2);
            _three = Input.GetKeyDown(KeyCode.Alpha3);
            _rotateItem = Input.GetKeyDown(KeyCode.R);
            _dropItem = Input.GetKeyDown(KeyCode.Q);
            _toggleSettings = Input.GetKeyDown(KeyCode.Escape);
            _toggleFishingBag = Input.GetKeyDown(KeyCode.E);
            _toggleCraftingKit = Input.GetKeyDown(KeyCode.C);

            if (Application.isFocused)
            {
                _gameplayMouse = _mouse;
                _moveDirection = Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw(HorizontalAxis), 0f, Input.GetAxisRaw(VerticalAxis)), 1f);
                _jump = Input.GetKeyDown(KeyCode.Space);
                _interact = Input.GetKeyDown(KeyCode.F);
            }
            else
            {
                _moveDirection = Vector3.zero;
                _jump = false;
                _interact = false;
            }
        }

        public bool TryGetScroll(out float scroll)
        {
            scroll = _scroll;
            return scroll != 0f;
        }

        public bool TryGetNumber(out int number)
        {
            number = -1;

            if (_one)
            {
                number = 1;
            }
            else if (_two)
            {
                number = 2;
            }
            else if (_three)
            {
                number = 3;
            }

            return number >= 1;
        }
    }
}