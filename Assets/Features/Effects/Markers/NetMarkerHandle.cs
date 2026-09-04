using UnityEngine;

namespace NoMoreFishAndChips.Effects
{
    public class NetMarkerHandle
    {
        private EnvironmentMarker _environmentMarker;

        private NetMarker _netMarker;
        private int _id;

        public NetMarkerHandle(EnvironmentMarker environmentMarker, NetMarker netMarker, int id)
        {
            _environmentMarker = environmentMarker;

            _netMarker = netMarker;
            _id = id;
        }

        public void SetPosition(Vector3 position)
        {
            if (position == _netMarker.GetPosition())
            {
                return;
            }
            
            _netMarker.SetPosition(position);
            SetDirty();
        }

        public void SetScale(Vector3 scale)
        {
            if (scale == _netMarker.GetScale())
            {
                return;
            }

            _netMarker.SetScale(scale);
            SetDirty();
        }

        public void SetBlend(float blend)
        {
            if (blend == _netMarker.Blend)
            {
                return;
            }

            _netMarker.SetBlend(blend);
            SetDirty();
        }

        private void SetDirty()
        {
            _environmentMarker.SetNetMarkerDirty(_id);
        } 

        public void Remove()
        {
            _environmentMarker.RemoveNetMarker(_id);
        }
    }
}