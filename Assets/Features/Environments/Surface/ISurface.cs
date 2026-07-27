using UnityEngine;

namespace NoMoreFishAndChips.Environments
{
    public enum SurfaceType
    {
        None,
        Wood,
        Sand,
        Grass,
        Goop
    }

    public interface ISurface
    {
        SurfaceType SurfaceType { get; }
    }
}