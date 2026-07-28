using TMPro;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class FloatingTextUI : WorldUI
    {
        [SerializeField] private TextMeshProUGUI _text;

        private UIManager _uiManager;

        private float _speed = 0.25f;
        private float _duration = 0.5f;
        private float _timer;

        private void Awake()
        {
            _uiManager = GameManager.Instance.Get<UIManager>();
        }

        public void Setup(string text)
        {
            _text.text = text;
        }

        private void Update()
        {
            transform.position += Vector3.up * _speed * Time.deltaTime;

            _timer += Time.deltaTime;

            if (_timer < _duration)
            {
                return;
            }

            _uiManager.DestroyWorldUI(this);
        }
    }
}