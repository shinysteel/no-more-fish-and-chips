using NoMoreFishAndChips.UI;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerContextLogic : RaftPlayerLogic
    {
        private UIManager _uiManager;

        private RaftPlayerContextSettings _settings;

        private ContextActionsUI _actionsUI;

        public RaftPlayerContextLogic(RaftPlayer player) : base(player)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();

            _settings = _player.DefinitionData.ContextSettings;
        }

        public void SetContext(ActionData[] datas)
        {
            if (datas != null)
            {
                _actionsUI ??= _uiManager.CreateWorldUI(_uiManager.Config.ContextActionsUIPrefab, Vector3.zero);
                _actionsUI.Setup(datas);
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
    }
}