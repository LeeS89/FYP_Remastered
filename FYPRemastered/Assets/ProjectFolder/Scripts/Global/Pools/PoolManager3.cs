using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public partial class PoolManager
{
    private ParticleSystem _particlePrefab;
    private ObjectPool<ParticleSystem> _particlePool;

    public PoolManager(ParticleSystem prefab, MonoBehaviour caller, int defaultSize = 5, int maxSize = 10)
        : this(defaultSize, maxSize)
    {
        this._particlePrefab = prefab;
        this._caller = caller;

        // Initialize the AudioSource pool
        _particlePool = new ObjectPool<ParticleSystem>(
            CreatePooledParticleSystem,
            OnGetFromPool,
            OnReturnToPool,
            OnDestroyPooledObject,
            false,
            _defaultSize,
            _maxPoolSize
        );
    }

    private void PrewarmParticlePoolPool(int count)
    {
        int prewarmCount = Mathf.Min(count, _maxPoolSize);
        List<ParticleSystem> tempList = new List<ParticleSystem>();

        
        for (int i = 0; i < prewarmCount; i++)
        {
            ParticleSystem obj = _particlePool.Get();
            tempList.Add(obj);
        }

        foreach (ParticleSystem obj in tempList)
        {
            _particlePool.Release(obj);
        }

        tempList.Clear();
    }

    private ParticleSystem CreatePooledParticleSystem()
    {
        ParticleSystem newObject = GameObject.Instantiate(_particlePrefab);
        newObject.transform.root.parent = _poolContainer.transform;
        return newObject; // Get AudioSource component from the prefab
    }

    private void OnGetFromPool(ParticleSystem particle)
    {
        particle.gameObject.SetActive(true); // Activate the AudioSource
    }

    private void OnReturnToPool(ParticleSystem particle)
    {
        particle.gameObject.SetActive(false); // Deactivate the AudioSource
    }

    private void OnDestroyPooledObject(ParticleSystem particle)
    {
        GameObject.Destroy(particle.gameObject); // Destroy the AudioSource object when it's no longer needed
    }

    public ParticleSystem GetParticle(Vector3 position, Quaternion rotation)
    {
        return GetObjectFromPool(_particlePool, position, rotation); 
    }

    private ParticleSystem GetObjectFromPool(ObjectPool<ParticleSystem> pool, Vector3 position, Quaternion rotation)
    {
        ParticleSystem obj = pool.Get();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        _caller.StartCoroutine(ReturnToPoolAfterDelay(obj, obj.main.duration));
        return obj;
    }

   
    private void ReleaseParticle(ParticleSystem particle)
    {
        ReleaseObjectToPool(_particlePool, particle); // Release an AudioSource back to the pool
    }

    private IEnumerator ReturnToPoolAfterDelay(ParticleSystem particle, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReleaseParticle(particle);
        //ReturnToPool(type, obj);
    }
}
