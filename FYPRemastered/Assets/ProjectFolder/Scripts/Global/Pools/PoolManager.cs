using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed class PoolManager<T> : PoolManagerBase /*IPoolManager*/ where T : UnityEngine.Object
{
    private ObjectPool<T> _pool;
    private int _maxSize;
    private Transform _poolContainer;
    private T _prefab;
    private Dictionary<T,Transform> _transformCache = new();
    private PoolResources _manager;
    private List<IPoolManager> _autoReleases = new(50);
    private List<PoolObjectTrackers> _jobs = new(50);

    public override Type ItemType => typeof(T);
    private static readonly bool IsPlainGameObject = (typeof(T) == typeof(GameObject));

    private static bool IsPrewarming;

    ///////  NEW CONSTRUCTOR
    public PoolManager(PoolResources manager, T prefab, int defaultCapacity = 10, int maxSize = 50)
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
            if (IsPrewarming) return;
            _jobs.Add(new PoolObjectTrackers(audio, audio.clip.length));
           // _manager?.SchedulePoolObjectRelease(this, audio, audio.clip.length);
        }else if(typeof(T) == typeof(ParticleSystem))
        {
            var ps = item as ParticleSystem;
            ps.gameObject.SetActive(false);
            if (IsPrewarming) return;
            _jobs.Add(new PoolObjectTrackers(ps, ps.main.duration));
            // _manager?.SchedulePoolObjectRelease(this, ps, ps.main.duration);
        }
        else if(typeof(T) == typeof(GameObject))
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
            Kind = PoolKind.Audio;
        }
        else if (typeof(T) == typeof(ParticleSystem))
        {
            var p = inst as ParticleSystem;
            var main = p.main;
            main.playOnAwake = false;
            _transformCache[inst] = p.transform;
            Kind = PoolKind.ParticleSystem;
        }
        else if (typeof(T) == typeof(GameObject))
        {
            var go = inst as GameObject;
            var poolable = go.GetComponentInChildren<IPoolable>();
            if (poolable != null)
                poolable.SetParentPool(this);

            Kind = PoolKind.GameObject;
            /* var eventManager = go.GetComponentInChildren<EventManager>();
             if (eventManager != null)
             {
                 eventManager.BindComponentsToEvents();
             }*/

            _transformCache[inst] = go.transform;
        }

        return inst;
    }

    ////// End of NEW CONSTRUCTOR
    public override void Tick()
    {
        if(IsPlainGameObject) return;

        if (_jobs == null || _jobs.Count == 0) { return; }

        float dt = Time.deltaTime;

        for (int i = _jobs.Count - 1; i >= 0; i--)
        {
            var job = _jobs[i];
            job.TimeRemaining -= dt;
            _jobs[i] = job;

            if (job.TimeRemaining <= 0f)
            {
                Release(job.Item);

                int last = _jobs.Count - 1;
                if (i != last)
                {
                    _jobs[i] = _jobs[last];
                }
                _jobs.RemoveAt(last);
            }

        }
    }


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
  

    public override void PreWarmPool(int count)
    {
        if (count <= 0) return;
        IsPrewarming = true;
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
        IsPrewarming = false;

    }

    public override UnityEngine.Object GetFromPool(Vector3 position, Quaternion rotation)
    {
        return Get(position, rotation);
    }

    public override void Release(UnityEngine.Object obj)
    {
        if(obj is T t) _pool.Release(t);
        else throw new InvalidOperationException(
            $"Cannot release object of type {obj.GetType()} to pool of type {typeof(T)}");
        
    }
}
