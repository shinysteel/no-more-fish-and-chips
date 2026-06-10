using NoMoreFishAndChips.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace NoMoreFishAndChips.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonSound : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private SoundId _id;

        private AudioManager _audioManager;

        private void Awake()
        {
            _audioManager = GameManager.Instance.Get<AudioManager>();

            _button.onClick.AddListener(Pressed);
        }

        private void Pressed()
        {
            _audioManager.PlaySound(_id);
        }
    }
}