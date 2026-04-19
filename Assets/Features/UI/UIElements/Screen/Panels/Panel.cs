using FishFlingers.Networking;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FishFlingers.UI
{
    public class PanelInstance<T> where T : Panel
    {
        private UIManager _uiManager;

        private T _prefab;
        private T _panel;
        private bool _creating;

        public PanelInstance(T prefab)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();

            _prefab = prefab;
        }

        public void Toggle(Action<T> onCreate)
        {
            if (_panel != null)
            {
                _panel.SimulateClosePressed();
                return;
            }

            if (!_creating)
            {
                _creating = true;

                _uiManager.CreateScreenUIAsync(_prefab, UILayer.Panels).completed += (T panel) =>
                {
                    _panel = panel;
                    _creating = false;

                    onCreate?.Invoke(_panel);
                    _panel.Show(null);
                };
            }
        }
    }

    public abstract class Panel : ScreenUI
    {
        [SerializeField] protected Button _closeButton;

        public override void Load(Canvas canvas)
        {
            base.Load(canvas);

            _closeButton.onClick.AddListener(ClosePressed);
        }

        protected void ClosePressed()
        {
            // Not every panel will need a reference to UIManager, so this is a one off
            // If more references came up, then I would consider defining all manager refs here
            Hide(() => GameManager.Instance.Get<UIManager>().DestroyScreenUI(this, UILayer.Panels));
        }

        public void SimulateClosePressed()
        {
            Utils.UI.SimulatePressed(_closeButton);
        }
    }
}