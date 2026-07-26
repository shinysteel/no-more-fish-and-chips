using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Pools;
using ShinyOwl.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    public class UsernameUI : WorldUI
    {
        [SerializeField] private TextMeshProUGUI _text;

        private PurrnetPlayer _player;

        public void Setup(PurrnetPlayer player)
        {
            _player = player;

            HandleUsernameChanged(_player.Username);

            _player.OnUsernameChanged += HandleUsernameChanged;
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnUsernameChanged -= HandleUsernameChanged;
            }   
        }

        private void Update()
        {
            if (_player?.RaftPlayer == null)
            {
                return;
            }

            transform.position = _player.RaftPlayer.transform.position + Vector3.up * 0.75f;
        }

        private void HandleUsernameChanged(string username)
        {
            _text.text = username;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_text.rectTransform);
        }
    }
}