using System;
using UnityEngine;

public static class ResourceExtensions
{
    public static PoolManagerNew<T> CreatePool<T>(this BulletResources manager, T prefab) where T : UnityEngine.Object
    {
        if (prefab == null) throw new ArgumentNullException(nameof(prefab));

        return prefab switch
        {
            AudioSource a => (PoolManagerNew<T>)(object) CreateNewAudioPool(manager, a),
            ParticleSystem p => (PoolManagerNew<T>)(object) CreateNewParticlePool(manager, p),
            GameObject g => (PoolManagerNew<T>)(object) CreateNewGOPool(manager, g),

            _ => throw new NotSupportedException($"No pool for {typeof(T).Name}")
        };

       /* return typeof(T) switch
           {
               var t when t == typeof(AudioSource)
               => (PoolManagerNew<T>)(object)CreateNewAudioPool(manager, prefab as AudioSource),

               var t when t == typeof(ParticleSystem)
               => (PoolManagerNew<T>)(object)CreateNewParticlePool(manager, prefab as ParticleSystem),

               _ => throw new NotSupportedException($"No pool for {typeof(T).Name}")
           };*/
    }
   


    private static PoolManagerNew<AudioSource> CreateNewAudioPool(this BulletResources manager, AudioSource prefab)
    {

        var poolRoot = new GameObject($"PoolRoot_{prefab.name}").transform;

        PoolManagerNew<AudioSource> pool = null;

        pool = new PoolManagerNew<AudioSource>(
          createFunc: () =>
          {
              var inst = UnityEngine.Object.Instantiate(prefab, poolRoot, false);
              inst.playOnAwake = false;
              IPoolable poolable = inst.GetComponentInChildren<IPoolable>();
              if (poolable != null)
              {
                  poolable.SetParentPool(pool);
              }
              return inst;
            
          },
          onGet: inst =>
          {
              inst.gameObject.SetActive(false);
              
          },
          onRelease: source =>
          {
              source.Stop();
              source.gameObject.SetActive(false);
          }
      );
        return pool;

    }

    private static PoolManagerNew<ParticleSystem> CreateNewParticlePool(BulletResources manager, ParticleSystem prefab)
    {

        var poolRoot = new GameObject($"PoolRoot_{prefab.name}").transform;
        PoolManagerNew<ParticleSystem> pool = null;

        pool = new PoolManagerNew<ParticleSystem>(
          createFunc: () =>
          {
              var inst = UnityEngine.Object.Instantiate(prefab, poolRoot, false);
              var main = inst.main;
              main.playOnAwake = false;
              IPoolable poolable = inst.GetComponentInChildren<IPoolable>();
              if (poolable != null)
              {
                  poolable.SetParentPool(pool);
              }

              return inst;
              
          },
          onGet: inst =>
          {
              inst.gameObject.SetActive(false);
              //source.Play();
          },
          onRelease: source =>
          {
              source.Stop();
              source.gameObject.SetActive(false);
          }
      );
        return pool;

    }



    private static PoolManagerNew<GameObject> CreateNewGOPool(BulletResources manager, GameObject prefab)
    {

        var poolRoot = new GameObject($"PoolRoot_{prefab.name}").transform;
        PoolManagerNew<GameObject> pool = null;

        pool = new PoolManagerNew<GameObject>(
          createFunc: () =>
          {
              var inst = UnityEngine.Object.Instantiate(prefab, poolRoot, false);
              IPoolable poolable = inst.GetComponentInChildren<IPoolable>();
              if (poolable != null)
              {
                  poolable.SetParentPool(pool);
              }
              return inst;
             
          },
          onGet: inst =>
          {
              inst.gameObject.SetActive(false);
              //source.Play();
          },
          onRelease: inst =>
          {
              inst.gameObject.SetActive(false);
          }
      );
        return pool;

    }




    //////////////////////////// END






    private static PoolManagerNew<AudioSource> CreateNewAudioPools(BulletResources manager, AudioSource source)
    {
        return new PoolManagerNew<AudioSource>(
          createFunc: () =>
          {
              GameObject go = new GameObject("PooledAudio");
              source = go.AddComponent<AudioSource>();
              
              source.playOnAwake = false;
              return source;
          },
          onGet: source =>
          {
              source.gameObject.SetActive(false);
            
          },
          onRelease: source =>
          {
              source.Stop();
              source.gameObject.SetActive(false);
          }
      );

    }

    private static PoolManagerNew<AudioSource> CreateNewParticlePool(BulletResources manager)
    {
        return new PoolManagerNew<AudioSource>(
          createFunc: () =>
          {
              GameObject go = new GameObject("PooledAudio");
              AudioSource source = go.AddComponent<AudioSource>();
              
              source.playOnAwake = false;
              return source;
          },
          onGet: source =>
          {
              source.gameObject.SetActive(false);
              //source.Play();
          },
          onRelease: source =>
          {
              source.Stop();
              source.gameObject.SetActive(false);
          }
      );

    }

    private static PoolManagerNew<T> CreateNewPool<T>(BulletResources manager) where T : UnityEngine.Object
    {
        return null;
    }
}
