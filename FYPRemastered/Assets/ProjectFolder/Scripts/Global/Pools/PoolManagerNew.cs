using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed class PoolManagerNew<T> : IPoolManager where T : UnityEngine.Object
{
    private ObjectPool<T> _pool;
    private int _maxSize;
    private Transform _poolContainer;
    private T _prefab;
    private Dictionary<T,Transform> _transformCache = new();
    private BulletResources _manager;

    public Type ItemType => typeof(T);

 
    ///////  NEW CONSTRUCTOR
    public PoolManagerNew(BulletResources manager, T prefab, int defaultCapacity = 10, int maxSize = 50)
    {
        _prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
        _poolContainer = new GameObject($"PoolContainer_{prefab.name}").transform;
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _maxSize = maxSize;

        _pool = new ObjectPool<T>(
            CreatePooledObject,
            OnGet,
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
            null, 
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
            _manager.SchedulePoolObjectRelease(this, ps, ps.main.duration);
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

            _transformCache[inst] = a.transform;
        }
        else if (typeof(T) == typeof(ParticleSystem))
        {
            var p = inst as ParticleSystem;
            var main = p.main;
            main.playOnAwake = false;
            _transformCache[inst] = p.transform;

        }
        else if (typeof(T) == typeof(GameObject))
        {
            var go = inst as GameObject;
            var poolable = go.GetComponentInChildren<IPoolable>();
            if (poolable != null)
                poolable.SetParentPool(this);

            _transformCache[inst] = go.transform;
        }

        return inst;
    }

    ////// End of NEW CONSTRUCTOR



    private T Get(Vector3 position, Quaternion rotation)
    {
        var item = _pool.Get();

        var tr = _transformCache[item];

        GameObject go = item switch
        {
            Component c => c.gameObject,
            GameObject g => g,
            _ => throw new InvalidOperationException(
                     $"{typeof(T)} is not a Component or GameObject")
        };

        tr.SetPositionAndRotation(position, rotation);
        go.SetActive(true);

        return item;
    }
  

    public void PreWarmPool(int count)
    {
        int preWarmCount = Mathf.Min(count, _maxSize);
        List<T> tempList = new List<T>();
        for (int i = 0; i < preWarmCount; i++)
        {
            var item = _pool.Get();
            tempList.Add(item);
        }
        Debug.LogError("Pool Count: "+tempList.Count);
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
