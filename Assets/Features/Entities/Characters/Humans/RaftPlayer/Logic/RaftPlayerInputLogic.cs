using PurrNet;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerInputLogic : RaftPlayerLogic
    {
        private SyncVar<Vector2> _netMousePositionNormalised;
        public Vector2 MousePositionNormalised => _netMousePositionNormalised.value;

        // Vars always active
        private Vector2 _mouse;
        private Vector3 _moveDirection;
        private bool _jump;
        private float _scroll;
        private bool _shiftHeld;
        private bool _leftClickPressed;
        private bool _leftClickHeld;
        private bool _rightClickPressed;
        private bool _fKey;
        private bool _onePressed;
        private bool _twoPressed;
        private bool _threePressed;
        private bool _fourPressed;
        private bool _fivePressed;
        private bool _rotateItem;
        private bool _dropItem;
        private bool _toggleSettings;
        private bool _toggleFishingBag;
        private bool _toggleCraftingKit;

        public Vector2 Mouse => _mouse;
        public Vector3 MoveDirection => _moveDirection;
        public bool Jump => _jump;
        public float Scroll => _scroll;
        public bool ShiftHeld => _shiftHeld;
        public bool LeftClickPressed => _leftClickPressed;
        public bool LeftClickHeld => _leftClickHeld;
        public bool RightClickPressed => _rightClickPressed;
        public bool FKey => _fKey;
        public bool OnePressed => _onePressed;
        public bool TwoPressed => _twoPressed;
        public bool ThreePressed => _threePressed;
        public bool FourPressed => _fourPressed;
        public bool FivePressed => _fivePressed;
        public bool RotateItem => _rotateItem;
        public bool DropItem => _dropItem;
        public bool ToggleSettings => _toggleSettings;
        public bool ToggleFishingBag => _toggleFishingBag;
        public bool ToggleCraftingKit => _toggleCraftingKit;

        public RaftPlayerInputLogic(RaftPlayer player, SyncVar<Vector2> netMousePositionNormalised) : base(player)
        {
            _netMousePositionNormalised = netMousePositionNormalised;
        }

        public override void Tick()
        {
            if (!_player.isOwner)
            {
                return;
            }

            if (Application.isFocused)
            {
                _mouse = Input.mousePosition;
                _netMousePositionNormalised.value = new Vector2(Mathf.Clamp01(_mouse.x / Screen.width), Mathf.Clamp01(_mouse.y / Screen.height));
            }

            _moveDirection = Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")), 1f);
            _jump = Input.GetKeyDown(KeyCode.Space);
            _scroll = Input.GetAxis("Mouse ScrollWheel");
            _shiftHeld = Input.GetKey(KeyCode.LeftShift);
            _leftClickPressed = Input.GetMouseButtonDown(0);
            _leftClickHeld = Input.GetMouseButton(0);
            _rightClickPressed = Input.GetMouseButtonDown(1);
            _fKey = Input.GetKeyDown(KeyCode.F);
            _onePressed = Input.GetKeyDown(KeyCode.Alpha1);
            _twoPressed = Input.GetKeyDown(KeyCode.Alpha2);
            _threePressed = Input.GetKeyDown(KeyCode.Alpha3);
            _fourPressed = Input.GetKeyDown(KeyCode.Alpha4);
            _fivePressed = Input.GetKeyDown(KeyCode.Alpha5);
            _rotateItem = Input.GetKeyDown(KeyCode.R);
            _dropItem = Input.GetKeyDown(KeyCode.Q);
            _toggleSettings = Input.GetKeyDown(KeyCode.Escape);
            _toggleFishingBag = Input.GetKeyDown(KeyCode.E);
            _toggleCraftingKit = Input.GetKeyDown(KeyCode.C);
        }

        public bool TryGetScroll(out float scroll)
        {
            scroll = _scroll;
            return scroll != 0f;
        }

        public bool TryGetNumber(out int number)
        {
            number = -1;

            if (_onePressed)
            {
                number = 1;
            }
            else if (_twoPressed)
            {
                number = 2;
            }
            else if (_threePressed)
            {
                number = 3;
            }
            else if (_fourPressed)
            {
                number = 4;
            }
            else if (_fivePressed)
            {
                number = 5;
            }

            return number >= 1;
        }
    }
}