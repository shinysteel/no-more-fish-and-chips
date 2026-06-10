using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "EntityManagerConfig", menuName = "Configs/Managers/EntityManagerConfig")]
    public class EntityManagerConfig : ScriptableObject
    {
        [SerializeField] private IEntityScanner _iEntityScanner;
        [SerializeField] private EntityModelScanner _entityModelScanner;

        public IEntityScanner IEntityScanner => _iEntityScanner;
        public EntityModelScanner EntityModelScanner => _entityModelScanner;
    }
}