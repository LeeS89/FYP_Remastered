using System;
using UnityEngine;

public abstract class EventManager : MonoBehaviour
{
    protected PoolManager _poolManager;

    public abstract void BindComponentsToEvents();

    public abstract void UnbindComponentsToEvents();

    public event Action OnOwnerDied;

    public void OwnerDied()
    {
        OnOwnerDied?.Invoke();
    }
}
