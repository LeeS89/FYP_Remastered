using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;

public enum PoolType
{
    NormalBullet,
    HitParticle,
    AudioSRC
}

public struct PoolSettings
{
    public GameObject prefab;
    public int initialCapacity;
    public int maxCapacity;
    public System.Type poolType; // The component type (AudioSource, ParticleSystem, GameObject)

    public PoolSettings(GameObject prefab, int initialCapacity, int maxCapacity, System.Type poolType)
    {
        this.prefab = prefab;
        this.initialCapacity = initialCapacity;
        this.maxCapacity = maxCapacity;
        this.poolType = poolType;
    }
}

public static class ParticlePool
{
    private static Dictionary<PoolType, object> _pools = new();
    private static Dictionary<PoolType, PoolSettings> _poolSettings = new();
    private static GameObject _poolContainer;
    private static MonoBehaviour _caller;

    public static void Initialize(Dictionary<PoolType, PoolSettings> poolData, MonoBehaviour caller)
    {
        if (_pools.Count > 0) { return; } // Prevent duplicate initialization

        _caller = caller;
        _poolContainer = new GameObject("PoolsContainer");
        _poolSettings = poolData;

        foreach (var kvp in poolData)
        {
            PoolType type = kvp.Key;
            PoolSettings settings = kvp.Value;

            if (settings.poolType == typeof(ParticleSystem))
            {
                CreatePool<ParticleSystem>(type, settings);
            }
            else if (settings.poolType == typeof(AudioSource))
            {
                CreatePool<AudioSource>(type, settings);
            }
            else if (settings.poolType == typeof(GameObject))
            {
              
                CreatePool(type, settings);
               
            }
            else
            {
                Debug.LogError($"Unsupported pool type: {settings.poolType}");
            }
        }
    }

    private static void CreatePool<T>(PoolType type, PoolSettings settings) where T : Component
    {
        _pools[type] = new ObjectPool<T>(
            createFunc: () =>
            {
                var obj = Object.Instantiate(settings.prefab).GetComponent<T>();
                obj.gameObject.SetActive(false);
                obj.transform.root.parent = _poolContainer.transform;
                return obj;
            },
            actionOnGet: (obj) => obj.gameObject.SetActive(true),

            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Object.Destroy(obj.gameObject),
            collectionCheck: false,
            defaultCapacity: settings.initialCapacity,
            maxSize: settings.maxCapacity
        );
    }

    private static void CreatePool(PoolType type, PoolSettings settings)
    {
        _pools[type] = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject obj = Object.Instantiate(settings.prefab);
                obj.SetActive(false);
                obj.transform.parent = _poolContainer.transform;
                return obj;
            },
            actionOnGet: (obj) => obj.SetActive(true),            
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Object.Destroy(obj),
            collectionCheck: false,
            defaultCapacity: settings.initialCapacity,
            maxSize: settings.maxCapacity
        );
    }

    public static T GetFromPool<T>(PoolType type, Vector3 position, Quaternion rotation) where T : Component
    {
        if (!_pools.ContainsKey(type))
        {
            Debug.LogError($"Pool for {type} does not exist!");
            return null;
        }

        var pool = _pools[type] as ObjectPool<T>;
        T obj = pool?.Get();

        if (obj != null)
        {
            
            obj.transform.SetPositionAndRotation(position, rotation);
           
            // If the object needs to be returned automatically, start a coroutine
            if (obj is ParticleSystem particle)
            {
                _caller.StartCoroutine(ReturnToPoolAfterDelay(type, particle, particle.main.duration));
            }
            else if (obj is AudioSource audio)
            {
                _caller.StartCoroutine(ReturnToPoolAfterDelay(type, audio, audio.clip.length));
            }
        }

        return obj;
    }

    public static GameObject GetFromPool(PoolType type, Vector3 position, Quaternion rotation)
    {
        if (!_pools.ContainsKey(type))
        {
            Debug.LogError($"Pool for {type} does not exist!");
            return null;
        }

        var pool = _pools[type] as ObjectPool<GameObject>;
        var obj = pool?.Get();

        if (obj != null)
        {
            obj.transform.parent = null;

            // Reset position and rotation explicitly
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            // Ensure it's active after setting its position
            obj.SetActive(true);

        }

        return obj;
    }

    private static IEnumerator ReturnToPoolAfterDelay<T>(PoolType type, T obj, float delay) where T : Component
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(type, obj);
    }

    public static void ReturnToPool<T>(PoolType type, T obj) where T : Component
    {
        if (!_pools.ContainsKey(type))
        {
            Debug.LogError($"Pool for {type} does not exist!");
            return;
        }

        var pool = _pools[type] as ObjectPool<T>;
        pool?.Release(obj);
    }

    public static void ReturnToPool(PoolType type, GameObject obj)
    {
        if (!_pools.ContainsKey(type))
        {
            Debug.LogError($"Pool for {type} does not exist!");
            return;
        }

        var pool = _pools[type] as ObjectPool<GameObject>;
        pool?.Release(obj);
    }
}
