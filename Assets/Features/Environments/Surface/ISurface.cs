using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public enum SurfaceType
    {
        None,
        Wood,
        Sand,
        Grass,
        Goop,
        Metal
    }

    public interface ISurface
    {
        SurfaceType SurfaceType { get; }
    }
}