using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerContextActionsLogic : RaftPlayerLogic
    {
        private UIManager _uiManager;
        private RaftPlayerContextActionsSettings _settings;

        private ActionData[] _actionDatas;
        private ContextActionsUI _actionsUI;

        public bool HasActions => _actionDatas != null;
        
        public RaftPlayerContextActionsLogic(RaftPlayer player) : base(player)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();

            _settings = _player.DefinitionData.ContextActionsSettings;
        }

        public void SetActionDatas(ActionData[] datas)
        {
            _actionDatas = datas;

            if (datas != null)
            {
                _actionsUI ??= _uiManager.CreateWorldUI(_uiManager.Config.ContextActionsUIPrefab, Vector3.zero);
                _actionsUI.Setup(_actionDatas);
            }
            else if (_actionsUI != null)
            {
                _uiManager.DestroyWorldUI(_actionsUI);
                _actionsUI = null;   
            }
        }

        public override void Tick()
        {
            if (_actionsUI != null)
            {
                _actionsUI.transform.position = _player.transform.position + Vector3.up * _settings.Offset;
            }
        }

        public void ListenForHotkeys()
        {
            foreach (ActionData data in _actionDatas)
            {
                if (_player.HotkeyLogic.IsActionHotkeyPressed(data.Hotkey))
                {
                    data.Execute(_context);
                }
            }
        }
    }
}