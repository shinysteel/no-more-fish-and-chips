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

using NetworkManager = NoMoreFishAndChips.Networking.NetworkManager;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System.Threading.Tasks;

namespace NoMoreFishAndChips.UI
{
    public class Chat : PurrBehaviour
    {
        [SerializeField] private CanvasGroup _backgroundCanvasGroup;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private CanvasGroup _messagesCanvasGroup;
        [SerializeField] private RectTransform _messageViewContainer;

        private NetworkManager _networkManager;
        private PoolManager _poolManager;

        private GameplayContext _context;

        private List<ChatMessage> _messages = new();
        private List<ChatMessageView> _messageViews = new();

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
        }

        private void Update()
        {
            if (_context.LocalPlayer.InputLogic.ToggleChat)
            {
                _inputField.Select();
            }
        }
    }
}