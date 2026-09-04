using UnityEngine;

namespace NoMoreFishAndChips.Effects
{
    public class NetMarker
    {
        private Vector2 _position;
        private Vector2 _scale;

        public float Blend { get; private set; }

        public NetMarker(Vector3 position, Vector3 scale, float blend)
        {
            SetPosition(position);
            SetScale(scale);
            SetBlend(blend);
        }

        public Vector3 GetPosition()
        {
            return new Vector3(_position.x, 0f, _position.y);
        }

        public Vector3 GetScale()
        {
            return new Vector3(_scale.x, 1f, _scale.y);
        }

        public void SetPosition(Vector3 position)
        {
            _position = new Vector2(position.x, position.z);
        }

        public void SetScale(Vector3 scale)
        {
            _scale = new Vector2(scale.x, scale.z);
        }

        public void SetBlend(float blend)
        {
            Blend = blend;
        }
    }
}