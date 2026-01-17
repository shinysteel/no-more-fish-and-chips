using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShinyOwl.Common.Framework;

public interface IGameSystem
{
    void Initialise(GameManagerConfig config);
    void Update();
    void LateUpdate();
    void Shutdown();
}

public enum ManagerState
{
    None,
    Initialising,
    Ready
}

/// <summary>
/// Generic game system that managers a feature
/// </summary>
/// <typeparam name="TListener"></typeparam>
public abstract class GameSystem<TListener> : IGameSystem
{
    public ManagerState State { get; protected set; }

    private ListenerHandler<TListener> _listeners = new();
    protected ListenerHandler<TListener> Listeners => _listeners;

    public virtual void Initialise(GameManagerConfig config)
    {
        State = ManagerState.Ready;
    }

    public virtual void Update()
    { }

    public virtual void LateUpdate()
    { }

    public virtual void Shutdown()
    {
        State = ManagerState.None;
    }

    public void AddListener(TListener listener)
    {
        _listeners.AddListener(listener);
    }

    public void RemoveListener(TListener listener)
    {
        _listeners.RemoveListener(listener);
    }
}