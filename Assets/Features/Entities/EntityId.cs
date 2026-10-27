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
        WoodenRaftTile = 201,
        ScaffoldRaftTile = 202,
        
        // Structures
        WaveCounter = 300,
        ClamChest = 301,
        Planter = 302,
        StructureScaffold = 303,
        TeslaCoil = 304,
        Cannon = 305,
        SalvageClaw = 306
    }
}