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
