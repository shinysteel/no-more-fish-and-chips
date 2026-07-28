using PurrNet;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class RaftPlayerLogicFactory : CharacterLogicFactory
    {
        private SyncVar<bool> _netInBarrel;

        public RaftPlayerLogicFactory(SyncVar<bool> netInBarrel) : base()
        {
            _netInBarrel = netInBarrel;
        }

        public override EntityDefeatLogic CreateDefeatLogic(Entity entity, SyncVar<bool> netIsDefeated)
        {
            return new RaftPlayerDefeatLogic((RaftPlayer)entity, netIsDefeated, _netInBarrel);
        }

        public override EntityPhysicsLogic CreatePhysicsLogic(Entity entity, Rigidbody rigidbody, NetworkRigidbody networkRigidbody, Collider collider)
        {
            return new RaftPlayerPhysicsLogic((RaftPlayer)entity, rigidbody, networkRigidbody, (CapsuleCollider)collider);
        }

        public override CharacterActLogic CreateActLogic(Character character)
        {
            return new RaftPlayerActLogic((RaftPlayer)character);
        }
    }
}