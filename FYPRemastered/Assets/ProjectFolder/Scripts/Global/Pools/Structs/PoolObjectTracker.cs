using UnityEngine;

public struct PoolObjectTracker
{
    public readonly IPoolManager Pool;
    public readonly UnityEngine.Object Item;
    public float TimeRemaining;

    public PoolObjectTracker(IPoolManager pool, UnityEngine.Object item, float timeRemaining)
    {
        this.Pool = pool;
        Item = item;
        TimeRemaining = timeRemaining;
    }
}

public struct PoolObjectTrackers
{
  
    public readonly UnityEngine.Object Item;
    public float TimeRemaining;

    public PoolObjectTrackers(UnityEngine.Object item, float timeRemaining)
    {
        Item = item;
        TimeRemaining = timeRemaining;
    }
}

