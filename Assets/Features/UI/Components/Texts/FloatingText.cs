using NoMoreFishAndChips.Pools;
using TMPro;
using UnityEngine;

namespace NoMoreFishAndChips.UI
{
    public class FloatingText : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private TextMeshPro _text;

        private PoolManager _poolManager;

        private float _speed = 0.25f;
        private float _duration = 0.5f;
        private float _timer;

        private void Awake()
        {
            _poolManager = GameManager.Instance.Get<PoolManager>();
        }

        public void OnTakenFromPool()
        {
            _timer = 0f;
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

            _poolManager.ReturnTypedPoolable(this);
        }

        public void OnReturnedToPool()
        { }
    }
}