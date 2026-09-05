using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Pools;
using NoMoreFishAndChips.States;
using PrimeTween;
using ShinyOwl.Common;
using UnityEngine;

namespace NoMoreFishAndChips.Effects
{
    public class Marker : MonoBehaviour, ITypedPoolable
    {
        [SerializeField] private MeshRenderer _meshRenderer;

        [SerializeField] private Gradient _gradient;

        private float _transformSpeed = 50f;
        private float _blendSpeed = 50f;

        private Material _material;

        private GameplayContext _context;

        private NetMarker _netMarker;
        private Tween _scaleTween;

        private NetMarker NetMarker => _netMarker;

        private Collider[] _collidersNonAlloc = new Collider[9];

        private void Awake()
        {
            _material = _meshRenderer.material;
        }

        public void Initialise(GameplayContext context, NetMarker netMarker)
        {
            _context = context;

            SetNetMarker(netMarker);

            PositionUpdate(1f);

            _scaleTween = Tween.Custom(startValue: 0f, endValue: 1f, duration: 0.5f, ease: Ease.OutBack, onValueChange: (float value) =>
            {
                transform.localScale = Vector3.LerpUnclamped(Vector3.zero, NetMarker.GetScale(), value);
            });

            BlendUpdate(1f);

            Tween.MaterialAlpha(_material, startValue: 0f, endValue: 1f, duration: 0.5f);
        }

        public void SetNetMarker(NetMarker netMarker)
        {
            _netMarker = netMarker;
        }

        private void Update()
        {
            PositionUpdate(Time.deltaTime);
            ScaleUpdate();
            BlendUpdate(Time.deltaTime);
        }

        private void PositionUpdate(float deltaTime)
        {
            float y = 0f;

            Vector3 overlapPosition = _netMarker.GetPosition();
            overlapPosition.y = 0.0625f;
            
            int overlaps = Physics.OverlapBoxNonAlloc(overlapPosition, _netMarker.GetScale() * 0.5f, _collidersNonAlloc, Quaternion.identity);

            for (int i = 0; i < overlaps; i++)
            {
                if (_collidersNonAlloc[i].TryGetComponent(out RaftTile tile))
                {
                    y = Mathf.Max(y, tile.transform.position.y);
                }
            }

            Vector3 targetPosition = _netMarker.GetPosition();
            targetPosition.y = y;

            transform.position = Vector3.Lerp(transform.position, targetPosition, _transformSpeed * deltaTime);
        }

        private void ScaleUpdate()
        {
            if (_scaleTween.isAlive)
            {
                return;
            }

            transform.localScale = Vector3.Lerp(transform.localScale, _netMarker.GetScale(), _transformSpeed * Time.deltaTime);
        }
        
        private void BlendUpdate(float deltaTime)
        {
            _material.color = Color.Lerp(_material.color, _gradient.Evaluate(_netMarker.Blend), _blendSpeed * deltaTime);
        }

        public void OnReturnedToPool()
        {
            Tween.StopAll(_material);
            Tween.StopAll(transform);

            _context = null;
        }

        public void OnTakenFromPool()
        { }
    }
}