using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public enum EntityId
    {
        None = 0,

        DroppedItem = 1,

        // Characters
        RaftPlayer = 100,
        Sailfin = 101,
        Shark = 102,
        Seagull = 103,
        Drowning = 104,
        Crab = 105,
        GiantClam = 106,
        Tentacle = 107,

        // Tiles
        GoopRaftTile = 201,
        WoodenRaftTile = 202,
        MetalRaftTile = 203,
        ScaffoldRaftTile = 204,
        
        // Structures
        WaveCounter = 300,
        ClamChest = 301,
        Planter = 302,
        StructureScaffold = 303,
        TeslaCoil = 304
    }
}