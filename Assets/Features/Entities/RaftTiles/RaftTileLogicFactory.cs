using PurrNet;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftTileLogicFactory : EntityLogicFactory
    {
        public override EntityDefeatLogic CreateDefeatLogic(Entity entity, SyncVar<bool> netIsDefeated)
        {
            return new RaftTileDefeatLogic((RaftTile)entity, netIsDefeated);
        }
    }
}