using FishFlingers.Scenes;
using NUnit.Framework;
using PrimeTween;
using ShinyOwl.Common;
using ShinyOwl.Common.Utils;
using Steam;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private IGameSystem[] _managers;
    private Dictionary<Type, int> _typeIndexMap = new();

    private GameManagerConfig _config;

    public static GameManager Instance { get; private set; }

    private const string ConfigPath = "GameManagerConfig";

    /// <summary>
    /// Needs to be in order of initialisation. Values need to be 1:1 with script names
    /// </summary>
    private enum Manager
    {
        SteamManager        ,
        NetworkManager      ,
        SceneManager        ,
        CameraManager       ,
        UIManager           ,
        StateManager        ,
        PoolManager         ,
        TransitionManager   ,
        DebugManager        ,
        LocalisationManager ,
        LobbyManager        ,
        ItemManager         ,
        EntityManager       ,
    }

    public TManager Get<TManager>() where TManager : IGameSystem
    {
        int index = _typeIndexMap[typeof(TManager)];
        return (TManager)_managers[index];
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitialiseManagers();

        Get<SceneManager>().LoadScene(EScene.Default);
    }

    private void InitialiseManagers()
    {
        Manager[] enums = (Manager[])Enum.GetValues(typeof(Manager));
        _managers = new IGameSystem[enums.Length];

        // Create the manager instances
        for (int i = 0; i < enums.Length; i++)
        {
            string typeName = enums[i].ToString();
            Type type = Type.GetType(typeName) ?? Utils.Files.FindTypeInAssembly(typeName); // Namespaces will hide classes from Type.GetType

            _managers[i] = (IGameSystem)Activator.CreateInstance(type);
            _typeIndexMap[type] = i;
        }

        _config = Resources.Load<GameManagerConfig>(ConfigPath);
        if (_config == null)
        {
            Debugger.LogError(this, "Unable to locate the GameManagerConfig at the specified path");
            return;
        }

        // Initialise them
        try
        {
            foreach (IGameSystem manager in _managers)
            {
                manager.Initialise(_config);
            }
        }
        catch (Exception ex)
        {
            Debugger.LogError(this, ex);
        }
    }

    private void Update()
    {
        foreach (IGameSystem manager in _managers)
        {
            manager.Tick();
        }
    }

    private void LateUpdate()
    {
        foreach (IGameSystem manager in _managers)
        {
            manager.LateTick();
        }
    }

    private void OnApplicationQuit()
    {
        Shutdown();
    }

    private void Shutdown()
    {
        foreach (IGameSystem manager in _managers)
        {
            manager.Shutdown();
        }

        Instance = null;
    }
}