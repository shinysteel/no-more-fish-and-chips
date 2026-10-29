using NoMoreFishAndChips.Pools;
using TMPro;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class ChatMessageView : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private TextMeshProUGUI _text;

        public void Setup(ChatMessage message)
        {
            _text.text = message.Text;
        }

        public void OnReturnedToPool()
        { }

        public void OnTakenFromPool()
        { }
    }
}