using NoMoreFishAndChips.UI;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public enum ChannelAnimation
    {
        None,
        Build,
        Craft
    }

    public class RaftPlayerActLogic : CharacterActLogic
    {
        private UIManager _uiManager;

        private bool _inCutscene;
        private Channel _channel;
        private float _channelTimer;

        public override bool CanAct => base.CanAct && !_inCutscene && _channel == null;

        private class Channel
        {
            public float Duration { get; private set; }
            public ChannelAnimation Animation { get; private set; }
            public Action Action { get; private set; }
            public ProgressBarUI UI { get; private set; }

            public Channel(float duration, ChannelAnimation animation, Action action, ProgressBarUI ui)
            {
                Duration = duration;
                Animation = animation;
                Action = action;
                UI = ui;
            }
        }

        public RaftPlayerActLogic(RaftPlayer player) : base(player)
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
        }

        public void SetInCutscene(bool cutscene)
        {
            _inCutscene = cutscene;
        }

        public void StartChannel(float duration, ChannelAnimation animation, Action action)
        {
            if (_channel != null)
            {
                return;
            }

            ProgressBarUI ui = _uiManager.CreateWorldUI(_uiManager.Config.ProgressBarUIPrefab, Vector3.zero);

            _channel = new Channel(duration, animation, action, ui);

            _channelTimer = 0f;

            _character.EntityModel.Animator.SetInteger(RaftPlayerAnimateLogic.ChannelAnimationIntName, (int)animation);
        }

        public override void Tick()
        {
            base.Tick();

            if (!_character.isOwner)
            {
                return;
            }

            if (_channel == null)
            {
                return;
            }

            _channelTimer += Time.deltaTime;
            _channelTimer = Mathf.Min(_channelTimer, _channel.Duration);

            if (_channelTimer < _channel.Duration)
            {
                _channel.UI.transform.position = _character.transform.position + Vector3.up * 0.75f;
                _channel.UI.SetFillAmount(_channelTimer / _channel.Duration);
            }
            else
            {
                _uiManager.DestroyWorldUI(_channel.UI);
                _channel.Action?.Invoke();
                _channel = null;
                _character.EntityModel.Animator.SetInteger(RaftPlayerAnimateLogic.ChannelAnimationIntName, 0);
            }
        }
    }
}