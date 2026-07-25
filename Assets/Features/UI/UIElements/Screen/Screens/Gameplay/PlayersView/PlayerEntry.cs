using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.Pools;
using TMPro;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class PlayerEntry : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private TextMeshProUGUI _usernameText;

        public void Setup(PurrnetPlayer purrnetPlayer)
        {
            _usernameText.text = purrnetPlayer.Username;
        }

        public void OnReturnedToPool()
        { }

        public void OnTakenFromPool()
        { }
    }
}