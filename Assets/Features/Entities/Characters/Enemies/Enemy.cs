using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class Enemy : Character
    {
        public abstract bool TrySpawn(SpawnParams parameters, GameplayContext context, out Enemy enemy);
    }

    public abstract class Enemy<T, U> : Enemy
        where T : EntityDefinitionData
        where U : EnemySpawnInfo
    {
        public T DefinitionData => (T)_entityDefinitionData;

        protected U _spawnInfo;
        public U SpawnInfo => _spawnInfo;

        public void SetSpawnInfo(U info)
        {
            _spawnInfo = info;
        }
    }
}