using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.Saving;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System.Threading.Tasks;
using TMPro;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class PlayerEntry : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _usernameText;
        [SerializeField] private Image _readyImage;
        [SerializeField] private Sprite _tickSprite;
        [SerializeField] private Sprite _crossSprite;

        private PurrnetPlayer _purrnetPlayer;
        private RaftPlayer _raftPlayer;

        private bool _isDirty;

        public void Setup(PurrnetPlayer player)
        {
            if (_purrnetPlayer != null)
            {
                _purrnetPlayer.OnUsernameChanged -= HandleUsernameChanged;
                _purrnetPlayer.OnRaftPlayerChanged -= HandleRaftPlayerChanged;
            }

            _purrnetPlayer = player;

            HandleUsernameChanged(_purrnetPlayer?.Username);
            HandleRaftPlayerChanged(_raftPlayer, _purrnetPlayer?.RaftPlayer);

            if (_purrnetPlayer != null)
            {
                _purrnetPlayer.OnUsernameChanged += HandleUsernameChanged;
                _purrnetPlayer.OnRaftPlayerChanged += HandleRaftPlayerChanged;
            }
        }

        private void HandleUsernameChanged(string username)
        {
            if (_usernameText.text == username)
            {
                return;
            }
            
            _usernameText.text = username;

            if (gameObject.activeInHierarchy)
            {
                _ = RebuildAsync();
            }
            else
            {
                _isDirty = true;
            }
        }

        private void HandleRaftPlayerChanged(RaftPlayer previous, RaftPlayer current)
        {
            if (previous != null)
            {
                previous.ReadyLogic.OnIsReadyChanged -= HandleIsReadyChanged;
            }

            _raftPlayer = current;

            HandleIsReadyChanged(_raftPlayer?.ReadyLogic.IsReady ?? false);

            if (_raftPlayer != null)
            {
                current.ReadyLogic.OnIsReadyChanged += HandleIsReadyChanged;
            }
        }

        private void HandleIsReadyChanged(bool ready)
        {
            _readyImage.sprite = ready ? _tickSprite : _crossSprite;
        }

        private void OnEnable()
        {
            if (_isDirty)
            {
                _ = RebuildAsync();
            }
        }

        private async Task RebuildAsync()
        {
            await Utils.Tasks.WaitForEndOfFrameAsync();           

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);

            _isDirty = false;
        }

        public void OnReturnedToPool()
        {
            Setup(null);
        }

        public void OnTakenFromPool()
        { }
    }
}