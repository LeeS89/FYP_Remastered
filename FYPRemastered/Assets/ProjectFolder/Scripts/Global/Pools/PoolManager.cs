using UnityEngine.Pool;
using UnityEngine;
using System.Collections.Generic;

public enum PoolContents
{
    Object,
    Particle,
    Audio
}

public partial class PoolManager
{
    private int _defaultSize;
    private int _maxPoolSize;
    //private MonoBehaviour _caller;
    private static GameObject _poolContainer;

    // Constructor that works for all object types
    public PoolManager(int defaultSize = 10, int maxSize = 20)
    {
        this._defaultSize = defaultSize;
        this._maxPoolSize = maxSize;

        if(_poolContainer == null)
        {
            _poolContainer = new GameObject("PoolContainer");
        }
    }

    // Common logic for prewarming the pool (retrieving and returning objects)
    /* public void PrewarmPool(ObjectPool<GameObject> pool, int count)
     {
         int prewarmCount = Mathf.Min(count, _maxPoolSize);
         List<GameObject> tempList = new List<GameObject>();

         Debug.LogError("PreWarm count is: " + prewarmCount);
         for (int i = 0; i < prewarmCount; i++)
         {
             GameObject obj = pool.Get();
             tempList.Add(obj);
         }

         foreach (GameObject obj in tempList)
         {
             pool.Release(obj);
         }

         tempList.Clear();
     }*/

    // Common methods for getting and returning objects
    /*public GameObject GetObjectFromPool(ObjectPool<GameObject> pool, Vector3 position, Quaternion rotation)
    {
        GameObject obj = pool.Get();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }*/

    public void PrewarmPool(PoolContents pool, int count)
    {
        switch (pool)
        {
            case PoolContents.Object:
                PrewarmObjectPool(count);
                break;
            case PoolContents.Particle:
                PrewarmParticlePoolPool(count);
                break;
            case PoolContents.Audio: 
                PrewarmAudioPool(count);
                break;
        }
    }

    public void ReleaseObjectToPool(ObjectPool<GameObject> pool, GameObject obj)
    {
        pool.Release(obj);
    }

    public void ReleaseObjectToPool(ObjectPool<AudioSource> pool, AudioSource obj)
    {
        pool.Release(obj);
    }

    public void ReleaseObjectToPool(ObjectPool<ParticleSystem> pool, ParticleSystem obj)
    {
        pool.Release(obj);
    }
    /*private GameObject _prefab;
    private ObjectPool<GameObject> _pool;
    private int _defaultSize;
    private int _maxPoolSize;

    
    public PoolManager(GameObject prefab, int defaultSize = 20, int maxSize = 50)
    {
        this._prefab = prefab;
        this._defaultSize = defaultSize;
        this._maxPoolSize = maxSize;

        _pool = new ObjectPool<GameObject>(
            CreatePooledObject,
            OnGetFromPool,
            OnReturnToPool,
            OnDestroyPooledObject,
            false,
            _defaultSize,
            _maxPoolSize
            );
    }

    public void PrewarmPool(int count)
    {
        // Ensure you don't exceed the max pool size
        int prewarmCount = Mathf.Min(count, _maxPoolSize);
        List<GameObject> tempList = new List<GameObject>();

        Debug.LogError("PreWarm count is: " + prewarmCount);
        // Pre-warm the pool by retrieving objects
        for (int i = 0; i < prewarmCount; i++)
        {
            // When getting an object, it is retrieved from the pool and added if the pool isn't full
            GameObject obj = _pool.Get();
            tempList.Add(obj);
        }

        foreach(GameObject obj in tempList)
        {
            _pool.Release(obj);
        }
        tempList.Clear();
        tempList = null;
    }

    public GameObject GetObject(Vector3 positon, Quaternion rotation)
    {
        GameObject obj = _pool.Get();
        obj.transform.position = positon;
        obj.transform.rotation = rotation;
        return obj;
    }

    public void ReleaseObject(GameObject obj)
    {
        _pool.Release(obj);
    }

    private GameObject CreatePooledObject()
    {
        GameObject newObject = GameObject.Instantiate(_prefab);

        IPoolable poolable = newObject.GetComponentInChildren<IPoolable>();
        if (poolable != null)
        {
            poolable.SetParentPool(this);  // Inject the pool reference into the object
        }


        return newObject;
    }

    private void OnGetFromPool(GameObject pooledObject)
    {
        pooledObject.SetActive(false);
    }

    private void OnReturnToPool(GameObject pooledObject)
    {
        pooledObject?.SetActive(false);
    }

    private void OnDestroyPooledObject(GameObject pooledObject)
    {
        GameObject.Destroy(pooledObject);
    }*/
}
