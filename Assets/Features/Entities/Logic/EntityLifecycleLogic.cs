using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public class EntityLifecycleLogic : EntityLogic
    {
        private EntityLifecycleSettings _settings;

        private float _spawnTime;
        private int _spawnFrame;

        public float TimeAlive => Time.time - _spawnTime;
        public int FramesAlive => Time.frameCount - _spawnFrame;

        public bool InGracePeriod => TimeAlive < _settings.GracePeriod;

        public EntityLifecycleLogic(Entity entity) : base(entity)
        {
            _settings = entity.EntityDefinitionData.EntityLifecycleSettings;

            _spawnTime = Time.time;
            _spawnFrame = Time.frameCount;
        }
    }
}