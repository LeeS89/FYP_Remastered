using System;
using UnityEngine;

public static class ResourceExtensions
{
    public static PoolManagerNew<T> CreatePool<T>(this BulletResources manager) where T : UnityEngine.Object
    { 
       // Type type = typeof(T);

        return typeof(T) switch
           {
               var t when t == typeof(AudioSource)
               => (PoolManagerNew<T>)(object)CreateNewAudioPool(manager),

               var t when t == typeof(ParticleSystem)
               => (PoolManagerNew<T>)(object)CreateNewPool<T>(manager),

               _ => throw new NotSupportedException($"No pool for {typeof(T).Name}")
           };
    }
    //return null;


    private static PoolManagerNew<AudioSource> CreateNewAudioPool(BulletResources manager, AudioClip clip = null)
    {
        return new PoolManagerNew<AudioSource>(
          createFunc: () =>
          {
              GameObject go = new GameObject("PooledAudio");
              AudioSource source = go.AddComponent<AudioSource>();
              if (clip != null)
              {
                  source.clip = clip;
              }
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
