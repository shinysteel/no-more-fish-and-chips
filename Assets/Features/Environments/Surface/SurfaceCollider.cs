using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public class SurfaceCollider : MonoBehaviour, ISurface
    {
        [SerializeField] private SurfaceType _surfaceType;

        SurfaceType ISurface.SurfaceType => _surfaceType;
    }
}