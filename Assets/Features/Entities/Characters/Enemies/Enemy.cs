using NoMoreFishAndChips.States;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    public abstract class Enemy : Character
    {
        public abstract bool TrySpawn(out Enemy enemy);
    }

    public abstract class Enemy<T> : Enemy
        where T : EntityDefinitionData
    {
        public T DefinitionData => (T)_entityDefinitionData;
    }

    public abstract class Enemy<T, U> : Enemy<T>
        where T : EntityDefinitionData
        where U : EnemySpawnInfo
    {
        protected U _spawnInfo;

        public void SetSpawnInfo(U info)
        {
            _spawnInfo = info;
        }
    }
}