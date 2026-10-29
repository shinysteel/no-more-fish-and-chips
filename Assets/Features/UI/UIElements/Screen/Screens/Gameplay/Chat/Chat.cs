using NoMoreFishAndChips.Networking;
using PurrNet;
using UnityEngine;
using System.Collections.Generic;
using NoMoreFishAndChips.Pools;
using ShinyOwl.Common.Utils;
using UnityEngine.UI;
using NoMoreFishAndChips.States;
using ShinyOwl.Common;
using TMPro;
using UnityEngine.EventSystems;
using System.Threading.Tasks;
using PrimeTween;

using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;

namespace NoMoreFishAndChips.UI
{
    public class Chat : PurrBehaviour
    {
        [SerializeField] private ScrollRect _messagesScrollRect;
        [SerializeField] private RectTransform _messageViewContainer;
        [SerializeField] private CanvasGroup _scrollBarCanvasGroup;
        [SerializeField] private CanvasGroup _inputContainerCanvasGroup;
        [SerializeField] private TMP_InputField _inputField;

        private NetworkManager _networkManager;
        private PoolManager _poolManager;

        private GameplayContext _context;

        private List<ChatMessage> _messages = new();
        private List<ChatMessageView> _messageViews = new();

        private Tween _scrollBarTween;
        private Tween _inputContainerTween;

        public bool InUse => _inputField.isFocused;

        protected override void Awake()
        {
            base.Awake();

            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _inputField.onSubmit.AddListener(HandleInputFieldSubmit);
            _inputField.onEndEdit.AddListener(HandleInputFieldEndEdit);
        }

        private void HandleInputFieldSubmit(string text)
        {
            if (!_inputField.isFocused)
            {
                return;
            }

            if (text == string.Empty)
            {
                return;
            }

            _networkManager.SendToAll(new ChatMessage() { Text = $"{_networkManager.LocalPurrnetPlayer.Username}: {text}" });
        }
        
        private void HandleInputFieldEndEdit(string text)
        {
            _inputField.text = string.Empty;

            async Task unselectAsync()
            {
                // We need to wait one frame, since Unity reselects the input field in this frame for some reason
                await Task.Yield();

                EventSystem.current.SetSelectedGameObject(null);

                _scrollBarTween.Stop();
                _scrollBarTween = Tween.Alpha(_scrollBarCanvasGroup, endValue: 0f, duration: 0.1f);
                
                _inputContainerTween.Stop();
                _inputContainerTween = Tween.Alpha(_inputContainerCanvasGroup, endValue: 0f, duration: 0.1f);
            }

            _ = unselectAsync();
        }

        public void Setup(GameplayContext context)
        {
            _context = context;
        }

        protected override void Subscribe(bool asServer)
        {
            _networkManager.Subscribe<ChatMessage>(HandleChatMessage, asServer);
        }

        protected override void Unsubscribe(bool asServer)
        {
            _networkManager.Unsubscribe<ChatMessage>(HandleChatMessage, asServer);
        }

        private void HandleChatMessage(PlayerID id, ChatMessage message, bool asServer)
        {
            _messages.Add(message);

            Utils.Collections.ResizeList(_messageViews, _messages.Count,
                createElement: () => _poolManager.GetTypedPoolable<ChatMessageView>(new SpawnParams() { Parent = _messageViewContainer }),
                removeElement: (ChatMessageView view) => _poolManager.ReturnTypedPoolable(view),
                processElement: (ChatMessageView view, int index) => view.Setup(_messages[index]));

            LayoutRebuilder.ForceRebuildLayoutImmediate(_messageViewContainer);
            
            async Task scrollAsync()
            {
                await Task.Yield();
                await Task.Yield();

                _messagesScrollRect.verticalNormalizedPosition = 0f;
            }

            _ = scrollAsync();
        }

        private void Update()
        {
            if (!_inputField.isFocused && _context.LocalPlayer.InputLogic.OpenChat)
            {
                _inputField.Select();

                _scrollBarTween.Stop();
                _scrollBarTween = Tween.Alpha(_scrollBarCanvasGroup, endValue: 1f, duration: 0.1f);

                _inputContainerTween.Stop();
                _inputContainerTween = Tween.Alpha(_inputContainerCanvasGroup, endValue: 1f, duration: 0.1f);
            }
        }
    }
}