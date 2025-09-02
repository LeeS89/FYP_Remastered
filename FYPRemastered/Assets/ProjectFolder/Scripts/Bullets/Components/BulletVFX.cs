using System;
using UnityEngine;

public class BulletVFX : ComponentEvents
{
    [SerializeField]
    private ParticleManager _particleManager;
    public ParticleSystem _particle;
    private IPoolManager _particlePoolManager;
    private IPoolManager _audioPoolManager;
    private ProjectileEventManager _bulletEventManager;

    public ParticleSystem particleMove;

    private ResourceRequest _request;
    private Action<PoolResourceType, IPoolManager> PoolRequestCallback;

    //public MoonSceneManager _manager;
    public override void RegisterLocalEvents(EventManager eventManager)
    {
        //Debug.LogError("Registering BulletVFX local events");
        _bulletEventManager = eventManager as ProjectileEventManager;
        PoolRequestCallback = OnPoolReceived;

        _particleManager = ParticleManager.instance;
        _bulletEventManager.OnDeflected += PlayDeflectionAudio;
        _bulletEventManager.OnBulletParticlePlay += PlayBulletParticle;
        _bulletEventManager.OnBulletParticleStop += StopBulletParticle;
        _bulletEventManager.OnCollision += SpawnHitParticle;

        
        //_bulletEventManager.OnSpawnHitParticle += SpawnHitParticle;
        /*_request = new ResourceRequest();
       

        _request.ResourceType = PoolResourceType.BasicHitParticlePool;
        _request.poolRequestCallback = OnPoolReceived;
        //_request.poolRequestCallback = (pool) =>
        //{

        //        _particlePoolManager = pool;

        //};

        SceneEventAggregator.Instance.RequestResource(_request);

        _request.ResourceType = PoolResourceType.DeflectAudioPool;
      *//*  _request.poolRequestCallback = (pool) =>
        {
            _audioPoolManager = pool;
        };*//*

        SceneEventAggregator.Instance.RequestResource(_request);*/
        //BaseSceneManager._instance.GetImpactParticlePool(ref _poolManager);

    }


    public override void InitialzeLocalPools()
    {

        var hitParticlePool = ResourceRequests.RequestPool(PoolResourceType.BasicHitParticlePool, PoolRequestCallback);
        SceneEventAggregator.Instance.ResourceRequested(hitParticlePool);

        var deflectAudioPool = ResourceRequests.RequestPool(PoolResourceType.DeflectAudioPool, PoolRequestCallback);
        SceneEventAggregator.Instance.ResourceRequested(deflectAudioPool);
        ///_request = new ResourceRequest();


    //    _request.ResourceType = PoolResourceType.BasicHitParticlePool;
     //   _request.poolRequestCallback = OnPoolReceived;
        //_request.poolRequestCallback = (pool) =>
        //{

        //        _particlePoolManager = pool;

        //};

     //   SceneEventAggregator.Instance.RequestResource(_request);

     //   _request.ResourceType = PoolResourceType.DeflectAudioPool;
        /*  _request.poolRequestCallback = (pool) =>
          {
              _audioPoolManager = pool;
          };*/

       // SceneEventAggregator.Instance.RequestResource(_request);
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _bulletEventManager.Expired();
        
        _bulletEventManager.OnDeflected -= PlayDeflectionAudio;
        _bulletEventManager.OnBulletParticlePlay -= PlayBulletParticle;
        _bulletEventManager.OnBulletParticleStop -= StopBulletParticle;
        _bulletEventManager.OnCollision -= SpawnHitParticle;
        //_bulletEventManager.OnSpawnHitParticle -= SpawnHitParticle;
        

        
    }

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        _request.OnInstanceDestroyed();
        _request = null;
        _particleManager = null;
        _bulletEventManager = null;
        PoolRequestCallback = null;
    }

    private void OnPoolReceived(PoolResourceType type, IPoolManager pool)
    {
        switch (type)
        {
            case PoolResourceType.BasicHitParticlePool:
                _particlePoolManager = pool;
                break;
                case PoolResourceType.DeflectAudioPool:
                _audioPoolManager = pool;
                break;
        }
        //_audioPoolManager = pool;
    }

    private void PlayDeflectionAudio(ProjectileKickType type)
    {
        // Different SFX for different deflection types

        var sfx = _audioPoolManager.Get(transform.position, transform.rotation) as AudioSource;
        sfx.Play();
        //PoolExtensions.GetAndPlay(_audioPoolManager, transform.position, transform.rotation);
    }
   

    private void SpawnHitParticle(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);
       // ContactPoint contact = collision.contacts[0];
        Vector3 pos = contact.point;
        //Vector3 hitNormal = contact.normal;
        //GameObject obj = _poolManager.GetFromPool(pos, rot);
        var hit = _particlePoolManager.Get(pos, Quaternion.identity) as ParticleSystem;
        hit.Play();
       // PoolExtensions.GetAndPlay(_particlePoolManager, pos, Quaternion.identity);
        //obj.SetActive(true);
        //_poolManager.GetParticle(pos, rot);  
    }

    private void PlayBulletParticle(Projectile bullet/*, BulletType bulletType*/)
    {
        //ParticleSystem particle = transform.root.GetComponentInChildren<ParticleSystem>();
        

        //particleMove.Clear(true); // Clear old particles
        //particleMove.Play(true);  // Force restart from frame 0

        //particleMove.Play();

        if (_particleManager == null) { return; }    

        _particleManager.AddBullet(bullet/*, bulletType*/);
    }

    private void StopBulletParticle(Projectile bullet/*, BulletType bulletType*/)
    {
        //ParticleSystem particle = transform.root.GetComponentInChildren<ParticleSystem>();
        /*particleMove.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);*/
        
        //particleMove.Pause();

        if (_particleManager == null) { return; }
        _particleManager.RemoveBullet(bullet);
    }

    private void OnDisable()
    {
        //particleMove.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        //particleMove.transform.localPosition = Vector3.zero;
    }

    /*private void OnEnable()
    {
        particleMove.transform.localPosition = Vector3.zero;
    }*/

   /* private void Update()
    {
        if (particleMove.isPlaying)
        {
            particleMove.transform.localPosition = Vector3.zero;
        }
    }*/

   

    void OnEnable()
    {
       // particleMove.Play(true);
        //particleMove.transform.localPosition = Vector3.zero;
        //particleMove.Clear(true);
        //_justActivated = true;
    }

   
}
