using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonSceneManager : BaseSceneManager
{
    [SerializeField] private ParticleSystem _normalHitPrefab;
    [SerializeField] private AudioSource _deflectAudioPrefab;
    [SerializeField] private GameObject _normalBulletPrefab;
    private PoolManager _bulletPool;
    private PoolManager _deflectAudioPool;
    private PoolManager _hitParticlePool;

    private void Start()
    {
        SetupScene();
    }

    public override void SetupScene()
    {
        LoadSceneResources();
        /* _normalBulletPrefab = Resources.Load<GameObject>("Bullets/NormalBullet");
         _normalHitPrefab = Resources.Load<ParticleSystem>("ParticlePoolPrefabs/BasicHit");
         _deflectAudioPrefab = Resources.Load<AudioSource>("AudioPoolPrefabs/DeflectAudio");*/

        InitializePools();
        LoadSceneEventManagers();
        //StartCoroutine(InitializeDelay());
        
    }

    IEnumerator InitializeDelay()
    {
        yield return new WaitForSeconds(1f);
        LoadSceneEventManagers();
    }

    protected override void LoadSceneResources()
    {
        _normalBulletPrefab = Resources.Load<GameObject>("Bullets/NormalBullet");
        _normalHitPrefab = Resources.Load<ParticleSystem>("ParticlePoolPrefabs/BasicHit");
        _deflectAudioPrefab = Resources.Load<AudioSource>("AudioPoolPrefabs/DeflectAudio");
    }
   

    private void InitializePools()
    {
        if(_normalBulletPrefab == null || _deflectAudioPrefab == null || _normalHitPrefab == null)
        {
            Debug.LogError("Failed to load Scene Resources");
            return;
        }

        _bulletPool = new PoolManager(_normalBulletPrefab);
        _bulletPool.PrewarmPool(PoolContents.Object, 5);

        _deflectAudioPool = new PoolManager(_deflectAudioPrefab, this, 5,10);

        _hitParticlePool = new PoolManager(_normalHitPrefab, this, 10, 20);
        _hitParticlePool.PrewarmPool(PoolContents.Particle, 5);
    }

    protected override void LoadSceneEventManagers()
    {
        _eventManagers = new List<EventManager>();

        _eventManagers.AddRange(FindObjectsByType<EventManager>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        foreach(var eventManager in _eventManagers)
        {
            if(eventManager is TestSpawn)
            {
                eventManager.HitParticlePoolInjection(_bulletPool);
            }
            

            eventManager.BindComponentsToEvents();
        }

        AssignPools();

    }

    private void AssignPools()
    {
        var impactParticleSubscribers = InterfaceRegistry.GetAll<IImpactVFX>();

        foreach(var particleSubscriber in impactParticleSubscribers)
        {
            particleSubscriber.SetImpactParticlePool(_hitParticlePool);
        }

        var impactAudioSubscribers = InterfaceRegistry.GetAll<IImpactAudio>();

        foreach(var audioSubscriber in impactAudioSubscribers)
        {
            audioSubscriber.SetDeflectAudioPool(_deflectAudioPool);
        }

    }
}
