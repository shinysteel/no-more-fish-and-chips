using System;
using System.Threading.Tasks;
using UnityEngine;

namespace FishFlingers.Entities
{
    public class EntityDefeatModule
    {
        private EntityManager _entityManager;

        private IEntity _entity;
        private float _defeatTime;

        private bool _isDefeated;

        public event Action OnDefeated;

        public EntityDefeatModule(IEntity entity, float defeatTime)
        {
            _entityManager = GameManager.Instance.Get<EntityManager>();

            _entity = entity;
            _defeatTime = defeatTime;
        }

        public void Defeat()
        {
            if (_isDefeated)
            {
                return;
            }

            _isDefeated = true;

            OnDefeated?.Invoke();

            _ = DespawnAsync();
        }

        private async Task DespawnAsync()
        {
            await Task.Delay(Mathf.RoundToInt(_defeatTime * 1000f));

            _entityManager.Despawn(_entity);
        }
    }
}
