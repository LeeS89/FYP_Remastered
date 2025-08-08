using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManagerNew<T> where T : UnityEngine.Object
{
    private ObjectPool<T> _pool;
    private int _maxSize;

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
    public void Release(T obj) => _pool.Release(obj); 

    public void PreWarmPool(int count)
    {
        int preWarmCount = Mathf.Min(count, _maxSize);

        for (int i = 0; i < preWarmCount; i++)
        {
            var item = _pool.Get();
            _pool.Release(item);
        }
        /* int preWarmCount = Mathf.Min(count, _maxSize);
         var tempList = new List<T>(preWarmCount);

         for(int i = 0; i < preWarmCount; i++)
         {
             var item = _pool.Get();
             tempList.Add(item);
         }

         foreach(var item in tempList)
         {
             _pool.Release(item);
         }*/

        /*tempList.Clear();
        tempList = null;*/
    }
}
