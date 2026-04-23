using FishFlingers.Entities;
using FishFlingers.Pools;
using FishFlingers.States;
using PrimeTween;
using UnityEngine;

namespace FishFlingers.Effects
{
    public class TileMark : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public void Initialise(GameplayContext context, Tile tile)
        {
            transform.SetParent(tile.transform);

            Vector3 position = tile.transform.position;
            position.y = tile.GetSurfaceY() + 0.01f;
            transform.position = position;

            Tween.Alpha(_spriteRenderer, startValue: 0f, endValue: 1f, duration: 0.5f);
            Tween.Scale(transform, startValue: Vector3.zero, endValue: Vector3.one, duration: 0.5f, ease: Ease.OutBack);
        }

        public void OnReturnedToPool()
        {
            Tween.StopAll(_spriteRenderer);
            Tween.StopAll(transform);
        }

        public void OnTakenFromPool()
        { }
    }
}