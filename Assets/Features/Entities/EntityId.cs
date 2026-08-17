using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public enum EntityId
    {
        None = 0,

        DroppedItem = 1,

        // Characters
        RaftPlayer = 100,
        FlyingFish = 101,
        Shark = 102,
        Seagull = 103,
        Drowning = 104,
        Crab = 105,
        GiantClam = 106,
        Tentacle = 107,

        // Tiles
        GoopTile = 201,
        WoodenTile = 202,
        MetalTile = 203,

        // Structures
        WaveCounter = 300,
        ClamChest = 301,
        Planter = 302,
    }
}