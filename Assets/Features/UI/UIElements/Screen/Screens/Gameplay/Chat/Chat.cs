using NoMoreFishAndChips.Inventories;
using NoMoreFishAndChips.Items;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using PrimeTween;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
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
        [SerializeField] private Button _sendButton;
        [SerializeField] private Color _usernameColor;

        private NetworkManager _networkManager;
        private PoolManager _poolManager;

        private GameplayContext _context;

        private List<ChatMessageView> _messageViews = new();

        public bool InUse => _inputField.isFocused;

        private const int MaxMessageViews = 20;

        protected override void Awake()
        {
            base.Awake();

            _networkManager = GameManager.Instance.Get<NetworkManager>();
            _poolManager = GameManager.Instance.Get<PoolManager>();

            _inputField.onSubmit.AddListener(HandleInputFieldSubmit);
            _inputField.onEndEdit.AddListener(HandleInputFieldEndEdit);

            _sendButton.onClick.AddListener(SendPressed);
        }

        private void HandleInputFieldSubmit(string text)
        {
            if (!_inputField.isFocused)
            {
                return;
            }

            Send();
        }
        
        private void HandleInputFieldEndEdit(string text)
        {
            async Task handleAsync()
            {
                // We need to wait one frame, since Unity reselects the input field in this frame for some reason
                await Task.Yield();

                EventSystem.current.SetSelectedGameObject(null);

                _scrollBarCanvasGroup.alpha = 0f;
                _inputContainerCanvasGroup.alpha = 0f;
            }

            _ = handleAsync();
        }

        private void SendPressed()
        {
            Send();
        }

        private void Send()
        {
            string text = _inputField.text;

            if (text == string.Empty)
            {
                return;
            }

            if (!text.StartsWith('/'))
            {
                _networkManager.SendToAll(new ChatMessage() { Username = _networkManager.LocalPurrnetPlayer.Username, Message = text });
            }
            else
            {
                ProcessCommand(text);
            }
        }

        private void ProcessCommand(string command)
        {
            string[] arguments = command.Split(' ');

            switch (arguments[0])
            {
                case "/give":
                    if (arguments.Length != 3)
                    {
                        return;
                    }

                    if (!Enum.TryParse(arguments[1], true, out ItemId id))
                    {
                        return;
                    }

                    if (!int.TryParse(arguments[2], out int count))
                    {
                        return;
                    }

                    InventoryChangeParams parameters = new InventoryChangeParams() { ItemId = id, Count = count };
                    bool result = _context.LocalPlayer.Inventory.TryAddItem(parameters, true, out _, out _, out _);

                    break;
            }
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
            string code = ColorUtility.ToHtmlStringRGB(_usernameColor);
            string text = $"<color=#{code}>{message.Username}</color>: {message.Message}";

            ChatMessageView view = _poolManager.GetTypedPoolable<ChatMessageView>(new SpawnParams() { Parent = _messageViewContainer });
            view.Setup(this, text);

            _messageViews.Add(view);

            if (_messageViews.Count > MaxMessageViews)
            {
                _poolManager.ReturnTypedPoolable(_messageViews[0]);
                _messageViews.RemoveAt(0);
            }

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
                _inputField.text = string.Empty;
                _inputField.Select();

                _scrollBarCanvasGroup.alpha = 1f;
                _inputContainerCanvasGroup.alpha = 1f;
            }
        }
    }
}