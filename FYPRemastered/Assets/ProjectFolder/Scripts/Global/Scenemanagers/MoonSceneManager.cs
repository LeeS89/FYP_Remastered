using System;
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
    public Dictionary<GameObject, float> stats = new Dictionary<GameObject, float>();

    public EnemyFSMController _enemy;
    public bool _testspawn = false;

    

    private void Start()
    {
        SetupScene();
    }

    private void Update()
    {
        if(_enemy == null) { return; }

        if (_testspawn)
        {
            _enemy.ResetFSM();
            _testspawn = false;
        }
    }

    public override void SetupScene()
    {
        LoadSceneResources();
       

        InitializePools();
        LoadActiveSceneEventManagers();
        //StartCoroutine(InitializeDelay());
        

        SceneStarted();
        
    }

    IEnumerator InitializeDelay()
    {
        yield return new WaitForSeconds(1f);
        LoadActiveSceneEventManagers();
    }

    protected override void LoadSceneResources()
    {
        _normalBulletPrefab = Resources.Load<GameObject>("Bullets/BasicBullet");
        _normalHitPrefab = Resources.Load<ParticleSystem>("ParticlePoolPrefabs/BasicHit");
        _deflectAudioPrefab = Resources.Load<AudioSource>("AudioPoolPrefabs/DeflectAudio");
        LoadWaypoints();
    }
   

    private void InitializePools()
    {
        if(_normalBulletPrefab == null || _deflectAudioPrefab == null || _normalHitPrefab == null)
        {
            Debug.LogError("Failed to load Scene Resources");
            return;
        }

        _hitParticlePool = new PoolManager(_normalHitPrefab, this, 40, 80);
        //_hitParticlePool.PrewarmPool(PoolContents.Particle, 15);

        _deflectAudioPool = new PoolManager(_deflectAudioPrefab, this, 5, 10);
        _deflectAudioPool.PrewarmPool(PoolContents.Audio, 5);

        _bulletPool = new PoolManager(_normalBulletPrefab, 40, 80);
        //_bulletPool.PrewarmPool(PoolContents.Object, 40);

        
       
    }

    protected override void LoadActiveSceneEventManagers()
    {
        _eventManagers = new List<EventManager>();

        _eventManagers.AddRange(FindObjectsByType<EventManager>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        foreach(var eventManager in _eventManagers)
        {
            if (eventManager is not BulletEventManager)
            {
                eventManager.BindComponentsToEvents();
            }
        }

        AssignPools();
        //GameManager.PlayerRespawned();
    }


    public override void GetImpactParticlePool(ref PoolManager manager)
    {
        manager = _hitParticlePool;  
    }

    public override void GetBulletPool(ref PoolManager manager)
    {
        manager = _bulletPool;
    }

    private void AssignPools()
    {
        
        var impactAudioSubscribers = InterfaceRegistry.GetAll<IImpactAudio>();

        foreach(var audioSubscriber in impactAudioSubscribers)
        {
            audioSubscriber.SetDeflectAudioPool(_deflectAudioPool);
        }

    }

    
}
