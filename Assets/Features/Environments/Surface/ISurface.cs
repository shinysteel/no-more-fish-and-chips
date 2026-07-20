using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public enum SurfaceType
    {
        None,
        Wood,
        Sand,
        Grass
    }

    public interface ISurface
    {
        SurfaceType SurfaceType { get; }
    }
}