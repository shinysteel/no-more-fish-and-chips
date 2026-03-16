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
using NetworkManager = FishFlingers.Networking.NetworkManager;
using Random = UnityEngine.Random;
using EntityId = FishFlingers.Entities.EntityId;

namespace FishFlingers.Saving
{
    [Serializable]
    public class UserSave
    {
        [JsonProperty] public string Guid { get; private set; }

        public UserSave()
        {
            Guid = System.Guid.NewGuid().ToString();
        }
    }

    [Serializable]
    public class GameSave
    {
        [JsonProperty] public Dictionary<string, RaftPlayerSave> Players { get; private set; } = new();
        [JsonProperty] public List<TileSave> Tiles { get; private set; } = new();
        [JsonProperty] public List<StructureSave> Structures { get; private set; } = new();
    }

    [Serializable]
    public class RaftPlayerSave
    {
        [JsonProperty] public SerialisableVector3 Position { get; private set; }
        [JsonProperty] public SerialisableQuaternion Rotation { get; private set; }

        private const int Precision = 1;

        public RaftPlayerSave(Vector3 position, Quaternion rotation)
        {
            position = Utils.Math.RoundVector3(position, Precision);
            rotation = Utils.Math.RoundQuaternion(rotation, Precision);

            Position = new SerialisableVector3(position);
            Rotation = new SerialisableQuaternion(rotation);
        }
    }

    [Serializable]
    public class TileSave
    {
        [JsonProperty] public SerialisableVector2Int Cell { get; private set; }
        [JsonProperty] public int Health { get; private set; }

        public TileSave(Vector2Int cell, int health)
        {
            Cell = new SerialisableVector2Int(cell);
            Health = health;
        }
    }

    [Serializable]
    public class StructureSave
    {
        [JsonProperty] public SerialisableVector2Int Cell { get; private set; }
        [JsonProperty] public EntityId StructureId { get; private set; }

        public StructureSave(Vector2Int cell, EntityId structureId)
        {
            Cell = new SerialisableVector2Int(cell);
            StructureId = structureId;
        }
    }

    public class SerialisableVector2Int
    {
        [JsonProperty] public int X { get; private set; }
        [JsonProperty] public int Y { get; private set; }

        public SerialisableVector2Int() : this(Vector2Int.zero)
        { }

        public SerialisableVector2Int(Vector2Int vector2Int)
        {
            X = vector2Int.x;
            Y = vector2Int.y;
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(X, Y);
        }
    }

    public class SerialisableVector3
    {
        [JsonProperty] public float X { get; private set; }
        [JsonProperty] public float Y { get; private set; }
        [JsonProperty] public float Z { get; private set; }

        public SerialisableVector3() : this(Vector3.zero) 
        { }

        public SerialisableVector3(Vector3 vector3)
        {
            X = vector3.x;
            Y = vector3.y;
            Z = vector3.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
    }

    public class SerialisableQuaternion
    {
        [JsonProperty] public float X { get; private set; }
        [JsonProperty] public float Y { get; private set; }
        [JsonProperty] public float Z { get; private set; }
        [JsonProperty] public float W { get; private set; }

        public SerialisableQuaternion() : this(Quaternion.identity)
        { }

        public SerialisableQuaternion(Quaternion quaternion)
        {
            X = quaternion.x;
            Y = quaternion.y;
            Z = quaternion.z;
            W = quaternion.w;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(X, Y, Z, W);
        }
    }

    public interface ISaveManagerListener
    { }

    public class SaveManager : GameSystem<ISaveManagerListener>
    {
        private NetworkManager _networkManager;

        private SaveManagerConfig _config;

        private string _persistentSavePath;
        private string _userSavePath;
        private string _gameSavePath;

        private UserSave _userSave;
        private GameSave _gameSave;

        public UserSave UserSave => _userSave;
        public GameSave GameSave => _gameSave;

        public override void Initialise(GameManagerConfig config)
        {
            _networkManager = GameManager.Instance.Get<NetworkManager>();

            _config = config.SaveManagerConfig;

            _persistentSavePath = CreatePersistentSavePath();

            _userSavePath = Path.Combine(_persistentSavePath, $"{_config.UserSaveFileName}.json");
            _gameSavePath = Path.Combine(_persistentSavePath, $"{_config.GameSaveFileName}.json");

            LoadUser();

            base.Initialise(config);
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
        public void LoadGame(GameplayContext context)
        {
            if (File.Exists(_gameSavePath))
            {
                string json = File.ReadAllText(_gameSavePath);
                _gameSave = JsonConvert.DeserializeObject<GameSave>(json);
            }
            else
            {
                _gameSave = new();

                // Start with a 3x3 grid
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        int health = Random.Range(NetTile.MaxHealth - 1, NetTile.MaxHealth + 1);
                        _gameSave.Tiles.Add(new TileSave(new Vector2Int(x, y), health));
                    }
                }

                // Start with a wave sign
                _gameSave.Structures.Add(new StructureSave(new Vector2Int(0, 1), EntityId.WaveSign));
            }

            foreach (TileSave save in _gameSave.Tiles)
            {
                context.Raft.AddNetTileRpc(save.Cell.ToVector2Int(), save.Health);
            }

            foreach (StructureSave save in _gameSave.Structures)
            {
                context.Raft.AddStructureRpc(save.Cell.ToVector2Int(), save.StructureId);
            }
        }

        public void SaveGame(GameplayContext context)
        {
            foreach (PurrnetPlayer purrnetPlayer in _networkManager.PurrnetPlayers.Values)
            {
                SaveRaftPlayer(purrnetPlayer.Guid, purrnetPlayer.RaftPlayer);
            }

            _gameSave.Tiles.Clear();
            
            foreach (RaftTile tile in context.Raft.RaftTiles.Values)
            {
                _gameSave.Tiles.Add(new TileSave(tile.Cell, tile.CurrentHealth));
            }

            _gameSave.Structures.Clear();

            foreach (RaftTile tile in context.Raft.RaftTiles.Values)
            {
                if (tile.Structure != null)
                {
                    _gameSave.Structures.Add(new StructureSave(tile.Cell, tile.Structure.StructureData.Id));
                }
            }

            string json = JsonConvert.SerializeObject(_gameSave, Formatting.Indented);
            File.WriteAllText(_gameSavePath, json);
        }

        public RaftPlayerSave GetRaftPlayerSave(string guid, GameplayContext context)
        {
            if (!_gameSave.Players.ContainsKey(guid))
            {
                _gameSave.Players[guid] = new RaftPlayerSave(Vector3.zero, Quaternion.identity);
            }

            return _gameSave.Players[guid];
        }

        public void LoadRaftPlayer(RaftPlayer raftPlayer, RaftPlayerSave save)
        {
            raftPlayer.transform.position = save.Position.ToVector3();
            raftPlayer.transform.rotation = save.Rotation.ToQuaternion();

            raftPlayer.Rigidbody.linearVelocity = Vector3.zero;
            raftPlayer.Rigidbody.angularVelocity = Vector3.zero;
        }
        
        public void SaveRaftPlayer(string guid, RaftPlayer raftPlayer)
        {
            _gameSave.Players[guid] = new RaftPlayerSave(raftPlayer.transform.position, raftPlayer.transform.rotation);
        }
    }
}