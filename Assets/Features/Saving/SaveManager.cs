using FishFlingers.Entities;
using FishFlingers.Environments;
using FishFlingers.Networking;
using FishFlingers.States;
using Newtonsoft.Json;
using ParrelSync;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FishFlingers.Instantiating;
using System.Threading.Tasks;
using UnityEngine.Pool;

namespace FishFlingers.Saving
{
    public interface ISaveable
    {
        Task LoadAsync();
        void Save();
    }

    public class UserSave
    {
        [JsonProperty] public string Guid { get; private set; }

        public UserSave()
        {
            Guid = System.Guid.NewGuid().ToString();
        }
    }

    public class GameSave
    {
        [JsonProperty] public Dictionary<string, PurrnetPlayerSave> Players { get; private set; } = new();
        [JsonProperty] public RaftSave Raft { get; private set; } = new();
    }

    public interface ISaveManagerListener
    { }

    public class SaveManager : GameSystem<ISaveManagerListener>, IInstantiateManagerListener
    {
        private InstantiateManager _gameObjectManager;

        private SaveManagerConfig _config;

        private string _persistentSavePath;
        private string _userSavePath;
        private string _gameSavePath;

        private UserSave _userSave;
        private GameSave _gameSave;

        public UserSave UserSave => _userSave;
        public GameSave GameSave => _gameSave;

        private List<ISaveable> _saveables = new();

        public override void Initialise(GameManagerConfig config)
        {
            _gameObjectManager = GameManager.Instance.Get<InstantiateManager>();

            _gameObjectManager.AddListener(this);

            _config = config.SaveManagerConfig;

            _persistentSavePath = CreatePersistentSavePath();

            _userSavePath = Path.Combine(_persistentSavePath, $"{_config.UserSaveFileName}.json");
            _gameSavePath = Path.Combine(_persistentSavePath, $"{_config.GameSaveFileName}.json");

            LoadUser();

            base.Initialise(config);
        }

        public override void Shutdown()
        {
            _gameObjectManager?.RemoveListener(this);

            base.Shutdown();
        }

        private string CreatePersistentSavePath()
        {
            string path = Application.persistentDataPath;

            if (ClonesManager.IsClone())
            {
                int cloneNumber = int.Parse(ClonesManager.GetCurrentProjectPath().Split($"{ClonesManager.CloneNameSuffix}_")[1]);
                path += $"{ClonesManager.CloneNameSuffix}_{cloneNumber}";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            return path;
        }

        /// <summary>
        /// Loads the user's details, which currently just contains their guid. No save equivalent is necessary, 
        /// since after first load this data will never change
        /// </summary>
        private void LoadUser()
        {
            if (File.Exists(_userSavePath))
            {
                string json = File.ReadAllText(_userSavePath);
                _userSave = JsonConvert.DeserializeObject<UserSave>(json);
            }
            else
            {
                _userSave = new();
                string json = JsonConvert.SerializeObject(_userSave);
                File.WriteAllText(_userSavePath, json);
            }
        }

        /// <summary>
        /// Loads a game save file. This includes data for players, other entities, and the raft
        /// </summary>
        public async Task LoadGameAsync()
        {
            if (File.Exists(_gameSavePath))
            {
                string json = File.ReadAllText(_gameSavePath);
                _gameSave = JsonConvert.DeserializeObject<GameSave>(json);
            }
            else
            {
                _gameSave = new();
                _gameSave.Raft.ApplyDefaults();
            }

            List<Task> tasks = ListPool<Task>.Get();

            try
            {
                foreach (ISaveable saveable in _saveables)
                {
                    tasks.Add(saveable.LoadAsync());
                }

                await Task.WhenAll(tasks);
            }
            finally
            {
                ListPool<Task>.Release(tasks);
            }
        }

        public void SaveGame()
        {
            foreach (ISaveable saveable in _saveables)
            {
                saveable.Save();
            }

            string json = JsonConvert.SerializeObject(_gameSave, Formatting.Indented);
            File.WriteAllText(_gameSavePath, json);
        }

        void IInstantiateManagerListener.OnComponentInstantiated(Component component)
        {
            if (component is ISaveable saveable)
            {
                _saveables.Add(saveable);
            }
        }

        void IInstantiateManagerListener.OnComponentDestroyed(Component component)
        {
            if (component is ISaveable saveable)
            {
                _saveables.Remove(saveable);
            }
        }
    }
}