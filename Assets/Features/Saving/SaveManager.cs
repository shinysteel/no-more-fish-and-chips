using NoMoreFishAndChips.Entities;
using NoMoreFishAndChips.Environments;
using NoMoreFishAndChips.Networking;
using NoMoreFishAndChips.States;
using Newtonsoft.Json;
using PurrNet;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NoMoreFishAndChips.Instantiating;
using System.Threading.Tasks;
using UnityEngine.Pool;
using System.Linq;

#if UNITY_EDITOR
using ParrelSync;
#endif

namespace NoMoreFishAndChips.Saving
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
        [JsonProperty] public GameplayEnvironmentSave Environment { get; private set; } = new();
    }

    public class SaveFile
    {
        private SaveManager _saveManager;

        private string _folderPath;
        private string _gameSavePath;
        private string _thumbnailPath;
        private string _name;

        public string FolderPath => _folderPath;
        public string GameSavePath => _gameSavePath;
        public string ThumbnailPath => _thumbnailPath;
        public string Name => _name;

        public SaveFile(string path)
        {
            _saveManager = GameManager.Instance.Get<SaveManager>();

            _folderPath = path;

            // Treat null as a new file to save to
            _folderPath ??= GetNextDefaultGameSaveFolderPath();

            _gameSavePath = Path.Combine(_folderPath, $"{_saveManager.Config.GameSaveFileName}.json");
            _thumbnailPath = Path.Combine(_folderPath, $"{_saveManager.Config.ThumbnailFileName}.png");

            _name = Path.GetFileName(_folderPath);
        }

        /// <summary>
        /// Retrieves the next path available to save a game to. Saves are formatted as 'New Raft', 'New Raft (1)', 'New Raft (2)'...
        /// </summary>
        private string GetNextDefaultGameSaveFolderPath()
        {
            int index = 0;

            while (true)
            {
                string path = Path.Combine(_saveManager.GameSavesFolderPath, _saveManager.Config.DefaultGameSaveFolderName);

                if (index > 0)
                {
                    path += $" ({index})";
                }

                if (Directory.Exists(path))
                {
                    index++;
                    continue;
                }

                return path;
            }
        }
    }

    public interface ISaveManagerListener
    { }

    public class SaveManager : GameSystem<ISaveManagerListener>, IInstantiateManagerListener
    {
        private InstantiateManager _gameObjectManager;

        private SaveManagerConfig _config;
        public SaveManagerConfig Config => _config;

        private string _persistentSaveFolderPath;
        private string _userSaveFilePath;
        private string _gameSavesFolderPath;

        public string GameSavesFolderPath => _gameSavesFolderPath;

        private UserSave _userSave;
        private GameSave _gameSave;

        public UserSave UserSave => _userSave;
        public GameSave GameSave => _gameSave;

        private List<SaveFile> _saveFiles = new();
        public IReadOnlyList<SaveFile> SaveFiles => _saveFiles;

        private SaveFile _selectedSaveFile;

        private List<ISaveable> _saveables = new();

        public override void Initialise(GameManagerConfig config)
        {
            _gameObjectManager = GameManager.Instance.Get<InstantiateManager>();

            _gameObjectManager.AddListener(this);

            _config = config.SaveManagerConfig;

            _persistentSaveFolderPath = CreatePersistentSavePath();

            _userSaveFilePath = Path.Combine(_persistentSaveFolderPath, $"{_config.UserSaveFileName}.json");
            _gameSavesFolderPath = Path.Combine(_persistentSaveFolderPath, _config.GameSavesFolderName);

            Directory.CreateDirectory(_gameSavesFolderPath);

            RefreshSaveFiles();

            LoadUser();

            base.Initialise(config);
        }

        public override void Shutdown()
        {
            _gameObjectManager?.RemoveListener(this);

            base.Shutdown();
        }

        private void RefreshSaveFiles()
        {
            _saveFiles.Clear();

            string[] paths = Directory.GetDirectories(_gameSavesFolderPath).OrderBy(path => File.GetCreationTime(path)).ToArray();

            foreach (string path in paths)
            {
                _saveFiles.Add(new SaveFile(path));
            }
        }

        private string CreatePersistentSavePath()
        {
            string path = Application.persistentDataPath;

#if UNITY_EDITOR
            if (ClonesManager.IsClone())
            {
                int cloneNumber = int.Parse(ClonesManager.GetCurrentProjectPath().Split($"{ClonesManager.CloneNameSuffix}_")[1]);

                path += $"{ClonesManager.CloneNameSuffix}_{cloneNumber}";

                Directory.CreateDirectory(path);
            }
#endif

            return path;
        }

        /// <summary>
        /// Loads the user's details, which currently just contains their guid. No save equivalent is necessary, 
        /// since after first load this data will never change
        /// </summary>
        private void LoadUser()
        {
            if (File.Exists(_userSaveFilePath))
            {
                string json = File.ReadAllText(_userSaveFilePath);
                _userSave = JsonConvert.DeserializeObject<UserSave>(json);
            }
            else
            {
                _userSave = new();
                string json = JsonConvert.SerializeObject(_userSave);
                File.WriteAllText(_userSaveFilePath, json);
            }
        }

        public void AddSaveFile(SaveFile file)
        {
            _saveFiles.Add(file);
        }

        public void SelectSaveFile(SaveFile file)
        {
            _selectedSaveFile = file;
        }

        /// <summary>
        /// Loads a game save file. This includes data for players, other entities, and the raft
        /// </summary>
        public async Task LoadGameAsync()
        {
            if (File.Exists(_selectedSaveFile.GameSavePath))
            {
                string json = File.ReadAllText(_selectedSaveFile.GameSavePath);
                _gameSave = JsonConvert.DeserializeObject<GameSave>(json);
            }
            else
            {
                _gameSave = new();
                _gameSave.Environment.ApplyDefaults();
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

        public async Task SaveGameAsync()
        {
            Directory.CreateDirectory(_selectedSaveFile.FolderPath);
            
            foreach (ISaveable saveable in _saveables)
            {
                saveable.Save();
            }
            
            string json = JsonConvert.SerializeObject(_gameSave, Formatting.Indented);
            File.WriteAllText(_selectedSaveFile.GameSavePath, json);

            ScreenCapture.CaptureScreenshot(_selectedSaveFile.ThumbnailPath);

            // Screenshots are queued to happen at the end of the frame
            await Task.Yield();
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