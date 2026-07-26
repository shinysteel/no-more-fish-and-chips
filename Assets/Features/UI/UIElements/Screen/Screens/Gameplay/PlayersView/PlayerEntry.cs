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

        private PurrnetPlayer _player;

        private bool _isDirty;

        public void Setup(PurrnetPlayer player)
        {
            if (_player != null)
            {
                _player.OnUsernameChanged -= HandleUsernameChanged;
                // _player.RaftPlayer.ReadyLogic.OnIsReadyChanged -= HandleIsReadyChanged;
            }

            _player = player;

            HandleUsernameChanged(_player?.Username);
            // HandleIsReadyChanged(_player?.RaftPlayer.ReadyLogic.IsReady ?? false);

            if (_player != null)
            {
                _player.OnUsernameChanged += HandleUsernameChanged;
                // _player.RaftPlayer.ReadyLogic.OnIsReadyChanged += HandleIsReadyChanged;
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