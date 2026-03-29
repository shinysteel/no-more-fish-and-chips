using FishFlingers.Entities;
using FishFlingers.Instantiating;
using FishFlingers.Items;
using FishFlingers.Saving;
using FishFlingers.States;
using Newtonsoft.Json;
using NUnit.Framework;
using ShinyOwl.Common;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using EntityId = FishFlingers.Entities.EntityId;

namespace FishFlingers.Environments
{
    public class GameplayEnvironmentSave
    {
        [JsonProperty] public RaftSave Raft { get; private set; } = new();
        [JsonProperty] public List<DroppedItemSave> DroppedItems { get; private set; } = new();

        public void ApplyDefaults()
        {
            Raft.ApplyDefaults();
        }
    }
    
    public class GameplayEnvironment : MonoBehaviour, ISaveable
    {
        private InstantiateManager _instantiateManager;
        private EntityManager _entityManager;
        private SaveManager _saveManager;

        private GameplayContext _context;

        private void Awake()
        {
            _instantiateManager = GameManager.Instance.Get<InstantiateManager>();
            _entityManager = GameManager.Instance.Get<EntityManager>();
            _saveManager = GameManager.Instance.Get<SaveManager>();
        }

        public void Initialise(GameplayContext context)
        {
            _context = context;

            _instantiateManager.RaiseComponentInstantiated(this);
        }

        private void OnDestroy()
        {
            _instantiateManager?.RaiseComponentDestroyed(this);
        }

        async Task ISaveable.LoadAsync()
        {
            _context.Raft.Load();

            foreach (DroppedItemSave save in _saveManager.GameSave.Environment.DroppedItems)
            {
                DroppedItem droppedItem = (DroppedItem)_entityManager.Spawn(EntityId.DroppedItem, new SpawnParams() { Position = save.Position });
                droppedItem.SetNetItemInstance(NetItemInstance.Create(save));
            }
        }

        void ISaveable.Save()
        {
            _context.Raft.Save();

            _saveManager.GameSave.Environment.DroppedItems.Clear();

            foreach (IEntity entity in _entityManager.Entities)
            {
                if (entity is not DroppedItem droppedItem)
                {
                    continue;
                }

                _saveManager.GameSave.Environment.DroppedItems.Add(new DroppedItemSave(droppedItem));
            }
        }
    }
}