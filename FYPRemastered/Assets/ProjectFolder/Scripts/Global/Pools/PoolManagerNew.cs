using System;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManagerNew<T> : IPoolManager where T : UnityEngine.Object
{
    private ObjectPool<T> _pool;
    private int _maxSize;

    public Type ItemType => typeof(T);

    public PoolManagerNew(Func<T> createFunc, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy = null, int defaultCapacity = 10, int maxSize = 50)
    {
        _maxSize = maxSize;

        _pool = new ObjectPool<T>(
            createFunc,
            onGet,
            onRelease,
            onDestroy,
            false,
            defaultCapacity,
            _maxSize);
    }

    public T Get(Vector3 position, Quaternion rotation)
    {
        var item = _pool.Get();

        GameObject go = item switch
        {
            Component c => c.gameObject,
            GameObject g => g,
            _ => throw new InvalidOperationException(
                     $"{typeof(T)} is not a Component or GameObject")
        };

        go.transform.SetPositionAndRotation(position, rotation);
        go.SetActive(true);

        return item;
    }
   // public void Release(T obj) => _pool.Release(obj); 

    public void PreWarmPool(int count)
    {
        int preWarmCount = Mathf.Min(count, _maxSize);

        for (int i = 0; i < preWarmCount; i++)
        {
            var item = _pool.Get();
            _pool.Release(item);
        }
       
    }

    UnityEngine.Object IPoolManager.Get(Vector3 position, Quaternion rotation)
    {
        return Get(position, rotation);
    }

    void IPoolManager.Release(UnityEngine.Object obj)
    {
        if(obj is T t) _pool.Release(t);
        else throw new InvalidOperationException(
            $"Cannot release object of type {obj.GetType()} to pool of type {typeof(T)}");
        
    }
}
