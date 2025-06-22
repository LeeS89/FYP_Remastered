/*using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


public partial class PoolManager
{
   *//* private GameObject _prefab;
    private ObjectPool<GameObject> _gameObjectPool;

    public PoolManager(GameObject prefab, int defaultSize = 20, int maxSize = 50)
        : this(defaultSize, maxSize)
    {
        this._prefab = prefab;

       
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

    private void PrewarmObjectPool(int count)
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

    private GameObject CreatePooledGameObject()
    {
        GameObject newObject = GameObject.Instantiate(_prefab);

        IPoolable poolable = newObject.GetComponentInChildren<IPoolable>();
        if (poolable != null)
        {
            poolable.SetParentPool(this);  
        }
        newObject.transform.root.parent = _poolContainer.transform;
        //Debug.LogError("Pre warmed bullet call create pooled object!!!");
        return newObject;
    }

    private void OnGetFromPool(GameObject obj)
    {
        obj.SetActive(false); ///// SET TO TRUE
    }

    private void OnReturnToPool(GameObject obj)
    {
        obj.SetActive(false); // Deactivate the GameObject
    }

    private void OnDestroyPooledObject(GameObject obj)
    {
        GameObject.Destroy(obj); // Destroy the GameObject when it's no longer needed
    }

   *//* public GameObject GetGameObject(Vector3 position, Quaternion rotation)
    {
        return GetObjectFromPool(position, rotation); // Get a GameObject from the pool
    }*//*

    public GameObject GetFromPool(Vector3 position, Quaternion rotation)
    {
        GameObject obj = _gameObjectPool.Get();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }

   *//* public void ReleaseToPool(GameObject obj)
    {
        ReleaseObjectToPool(_gameObjectPool, obj); // Release a GameObject back to the pool
    }*//*
}
*/