using UnityEngine.Pool;
using UnityEngine;
using System.Collections.Generic;



public class PoolManager
{
    private int _defaultSize;
    private int _maxPoolSize;
  
    private static GameObject _poolContainer;
    private GameObject _prefab;
    private ObjectPool<GameObject> _gameObjectPool;

    public PoolManager(GameObject prefab, int defaultSize = 20, int maxSize = 50)
    {
        this._defaultSize = defaultSize;
        this._maxPoolSize = maxSize;
        this._prefab = prefab;

        if (_poolContainer == null)
        {
            _poolContainer = new GameObject("PoolContainer");
        }

        _gameObjectPool = new ObjectPool<GameObject>(
            CreatePooledGameObject,
            OnGetFromPool,
            OnReturnToPool,
            OnDestroyPooledObject,
            false,
            _defaultSize,
            _maxPoolSize
        );
    }

    private GameObject CreatePooledGameObject()
    {
        GameObject newObject = GameObject.Instantiate(_prefab);

        IPoolable poolable = newObject.GetComponentInChildren<IPoolable>();
        if (poolable != null)
        {
            //poolable.SetParentPool(this);
        }
        newObject.transform.root.parent = _poolContainer.transform;
       
        return newObject;
    }

    public void PrewarmPool(int count)
    {
        int prewarmCount = Mathf.Min(count, _maxPoolSize);
        List<GameObject> tempList = new List<GameObject>();


        for (int i = 0; i < prewarmCount; i++)
        {
            GameObject obj = _gameObjectPool.Get();
            tempList.Add(obj);
        }

        foreach (GameObject obj in tempList)
        {
            _gameObjectPool.Release(obj);
        }

        tempList.Clear();
        tempList = null;
    }

    public GameObject GetFromPool(Vector3 position, Quaternion rotation)
    {
        GameObject obj = _gameObjectPool.Get();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    private void OnGetFromPool(GameObject obj)
    {
        obj.SetActive(false); ///// SET TO TRUE
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

    /* public void PrewarmPool(PoolContents pool, int count)
     {
         switch (pool)
         {
             case PoolContents.Object:
                 PrewarmObjectPool(count);
                 break;

         }
     }*/

    public void ReleaseObjectToPool(GameObject obj)
    {
        _gameObjectPool.Release(obj);
    }

    private void OnReturnToPool(GameObject obj)
    {
        obj.SetActive(false); // Deactivate the GameObject
    }

   
    private void OnDestroyPooledObject(GameObject obj)
    {
        GameObject.Destroy(obj); // Destroy the GameObject when it's no longer needed
    }

    /* public GameObject GetGameObject(Vector3 position, Quaternion rotation)
     {
         return GetObjectFromPool(position, rotation); // Get a GameObject from the pool
     }*/

   

    /* public void ReleaseToPool(GameObject obj)
     {
         ReleaseObjectToPool(_gameObjectPool, obj); // Release a GameObject back to the pool
     }*/

}
