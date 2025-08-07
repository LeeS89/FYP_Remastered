using System;
using UnityEngine.Pool;

public class PoolManagerNew<T> where T : class
{
    private ObjectPool<T> _pool;

    public PoolManagerNew(Func<T> createFunc, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy = null, int defaultCapacity = 10, int maxSize = 50)
    {
        _pool = new ObjectPool<T>(
            createFunc,
            onGet,
            onRelease,
            onDestroy,
            false,
            defaultCapacity,
            maxSize);
    }

    public T Get() => _pool.Get();
    public void Release(T obj) => _pool.Release(obj); 
}
