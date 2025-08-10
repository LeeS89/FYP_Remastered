using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public sealed class PoolManagerNew<T> : IPoolManager where T : UnityEngine.Object
{
    private ObjectPool<T> _pool;
    private int _maxSize;
    private Transform _poolContainer;
    private T _prefab;
    BulletResources _manager;

    public Type ItemType => typeof(T);

 /*   public PoolManagerNew(Func<T> createFunc, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy = null, int defaultCapacity = 10, int maxSize = 50)
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
    }*/


    ///////  NEW CONSTRUCTOR
    public PoolManagerNew(BulletResources manager, T prefab, int defaultCapacity = 10, int maxSize = 50)
    {
        _prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
        _poolContainer = new GameObject($"PoolContainer_{prefab.name}").transform;
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));

        _pool = new ObjectPool<T>(
            CreatePooledObject,
            item =>
            {
                if (item is GameObject go)
                {
                    go.SetActive(false);
                }
                else if (item is Component c)
                {
                    c.gameObject.SetActive(false);
                }
            },
            item => 
            {
                if (item is GameObject go)
                {
                    go.SetActive(false);
                }
                else if (item is Component c)
                {
                    c.gameObject.SetActive(false);
                }
            }, // No specific release action needed
            null, // No destroy action needed
            false,
            defaultCapacity,
            maxSize);
    }

    private void OnGet(T item)
    {
        if (typeof(T) == typeof(AudioSource))
        {
            var audio = item as AudioSource;
            audio.gameObject.SetActive(false);
            _manager.SchedulePoolObjectRelease(this, audio, audio.clip.length);
        }else if(typeof(T) == typeof(ParticleSystem))
        {
            var ps = item as ParticleSystem;
            ps.gameObject.SetActive(false);
            _manager.SchedulePoolObjectRelease(this, ps.gameObject, ps.main.duration);
        }else if(typeof(T) == typeof(GameObject))
        {
            var go = item as GameObject;
            go.SetActive(false);
        }
    }

    private T CreatePooledObject()
    {
        var inst = UnityEngine.Object.Instantiate(_prefab, _poolContainer, false);

        if (typeof(T) == typeof(AudioSource))
        {
            var a = inst as AudioSource;
            a.playOnAwake = false;
        }
        else if (typeof(T) == typeof(ParticleSystem))
        {
            var p = inst as ParticleSystem;
            var main = p.main;
            main.playOnAwake = false;

        }
        else if (typeof(T) == typeof(GameObject))
        {
            var poolable = inst.GetComponentInChildren<IPoolable>();
            if (poolable != null)
                poolable.SetParentPool(this);
        }

        return inst;
    }

    ////// End of NEW CONSTRUCTOR



    private T Get(Vector3 position, Quaternion rotation)
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
        List<T> tempList = new List<T>();
        for (int i = 0; i < preWarmCount; i++)
        {
            var item = _pool.Get();
            tempList.Add(item);
        }

        foreach (var item in tempList)
        {
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
