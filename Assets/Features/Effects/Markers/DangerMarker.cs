using FishFlingers.Entities;
using FishFlingers.Pools;
using FishFlingers.States;
using PrimeTween;
using ShinyOwl.Common;
using UnityEngine;

namespace FishFlingers.Effects
{
    public class DangerMarker : MonoBehaviour, IPoolable
    {
        [SerializeField] private Transform _visualTransform;
        [SerializeField] private MeshRenderer _meshRenderer;

        private Material _material;

        private GameplayContext _context;
        private Vector2Int[] _cells;

        private void Awake()
        {
            _material = _meshRenderer.material;
        }

        public void Initialise(GameplayContext context, Vector2Int[] cells)
        {
            _context = context;
            _cells = cells;

            InitialiseTransform();

            Tween.MaterialAlpha(_material, startValue: 0f, endValue: 1f, duration: 0.5f);
            Tween.Scale(_visualTransform, startValue: Vector3.zero, endValue: Vector3.one, duration: 0.5f, ease: Ease.OutBack);
        }

        private void InitialiseTransform()
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;

            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (Vector2Int cell in _cells)
            {
                minX = Mathf.Min(minX, cell.x);
                minY = Mathf.Min(minY, cell.y);

                maxX = Mathf.Max(maxX, cell.x);
                maxY = Mathf.Max(maxY, cell.y);
            }

            transform.position = new Vector3((minX + maxX) / 2f, 0f, (minY + maxY) / 2f);
            transform.localScale = new Vector3(maxX - minX + 1, 1f, maxY - minY + 1);
        }

        private void Update()
        {
            float? y = null;

            for (int i = 0; i < _cells.Length; i++)
            {
                if (_context.Raft.Tiles.TryGetValue(_cells[i], out Tile tile))
                {
                    y = Mathf.Max(y ?? float.MinValue, tile.GetSurfaceY());
                }
            }

            y ??= 0.125f;

            Vector3 position = transform.position;
            position.y = y.Value;

            transform.position = position;
        }

        public void OnReturnedToPool()
        {
            Tween.StopAll(_material);
            Tween.StopAll(_visualTransform);

            _context = null;
            _cells = null;
        }

        public void OnTakenFromPool()
        { }
    }
}