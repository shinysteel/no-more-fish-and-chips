using PrimeTween;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class Drowning : Character<DrowningDefinitionData>
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private AnimationCurve _scaleCurve;

        private RaftPlayer _target;

        public void SetTarget(RaftPlayer target)
        {
            _target = target;

            Vector3 direction = _target.transform.position;
            direction.y = 0f;
            direction.Normalize();

            direction = Quaternion.AngleAxis(Random.Range(-30f, 30f), Vector3.up) * direction;

            Vector3 position = _target.transform.position;
            position.y = 0f;
            position += direction * 3f;

            transform.position = position;
        }

        protected override void OnEarlySpawn()
        {
            Tween.Alpha(_spriteRenderer, startValue: 0f, endValue: 1f, duration: 0.33f);
        }

        protected override void Update()
        {
            base.Update();

            ScaleUpdate();

            if (!isServer)
            {
                return;
            }

            MoveUpdate();
        }

        private void ScaleUpdate()
        {
            float speed = Mathf.Pow(1.1f, _entityLifecycleModule.TimeAlive);
            float time = _entityLifecycleModule.TimeAlive * speed % 1f;
            float scale = 1f + 0.2f * _scaleCurve.Evaluate(time);
            
            transform.localScale = Vector3.one * scale;
        }

        private void MoveUpdate()
        {
            Vector3 direction = (_target.transform.position - transform.position);
            direction.y = 0f;
            direction.Normalize();

            float speed = -1f + Mathf.Pow(1.4f, _entityLifecycleModule.TimeAlive);

            transform.position += direction * speed * Time.deltaTime;
        }
    }
}