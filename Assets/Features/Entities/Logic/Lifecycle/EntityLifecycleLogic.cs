using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class EntityLifecycleLogic : EntityLogic
    {
        private EntityLifecycleSharedSettings _sharedSettings;

        private float _spawnTime;
        private int _spawnFrame;

        public float TimeAlive => Time.time - _spawnTime;
        public int FramesAlive => Time.frameCount - _spawnFrame;

        public bool InGracePeriod => TimeAlive < _sharedSettings.GracePeriod;

        public EntityLifecycleLogic(Entity entity) : base(entity)
        {
            _sharedSettings = entity.EntityDefinitionData.EntityLifecycleSharedSettings;

            _spawnTime = Time.time;
            _spawnFrame = Time.frameCount;
        }
    }
}