using System;
using UnityEngine;

namespace NoMoreFishAndChips.Entities
{
    [CreateAssetMenu(fileName = "EntityManagerConfig", menuName = "Configs/Managers/EntityManagerConfig")]
    public class EntityManagerConfig : ScriptableObject
    {
        [SerializeField] private EntityScanner _entityScanner;
        [SerializeField] private EntityModelScanner _entityModelScanner;

        public EntityScanner EntityScanner => _entityScanner;
        public EntityModelScanner EntityModelScanner => _entityModelScanner;
    }
}