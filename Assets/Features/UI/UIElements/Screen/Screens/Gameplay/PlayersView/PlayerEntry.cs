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

        private PurrnetPlayer _purrnetPlayer;

        private bool _isDirty;

        public void Setup(PurrnetPlayer purrnetPlayer)
        {
            if (_purrnetPlayer != null)
            {
                _purrnetPlayer.OnUsernameChanged -= HandleUsernameChanged;
            }

            _purrnetPlayer = purrnetPlayer;

            HandleUsernameChanged(_purrnetPlayer?.Username);

            if (_purrnetPlayer == null)
            {
                return;
            }

            _purrnetPlayer.OnUsernameChanged += HandleUsernameChanged;
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