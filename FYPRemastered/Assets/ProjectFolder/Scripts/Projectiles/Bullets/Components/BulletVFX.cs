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

    [SerializeField] public PoolIdSO DeflectAudioPoolId;
    public PoolIdSO HitParticlePoolId;


    private Action<PoolIdSO, IPoolManager> PoolRequestCallback;

    public override void RegisterLocalEvents(EventManager eventManager)
    {

        _bulletEventManager = eventManager as ProjectileEventManager;
        PoolRequestCallback = OnPoolReceived;

        _particleManager = ParticleManager.instance;
        _bulletEventManager.OnDeflected += PlayDeflectionAudio;
        _bulletEventManager.OnProjectileParticlePlay += PlayBulletParticle;
        _bulletEventManager.OnProjectileParticleStop += StopBulletParticle;
        _bulletEventManager.OnCollision += SpawnHitParticle;


    }


    public override void InitialzeLocalPools()
    {
        this.RequestPool(HitParticlePoolId, PoolRequestCallback);
       
        this.RequestPool(DeflectAudioPoolId, PoolRequestCallback);
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _bulletEventManager.Expired();
        
        _bulletEventManager.OnDeflected -= PlayDeflectionAudio;
        _bulletEventManager.OnProjectileParticlePlay -= PlayBulletParticle;
        _bulletEventManager.OnProjectileParticleStop -= StopBulletParticle;
        _bulletEventManager.OnCollision -= SpawnHitParticle;    
    }

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        _particleManager = null;
        _bulletEventManager = null;
        PoolRequestCallback = null;
    }

    private void OnPoolReceived(PoolIdSO poolId, IPoolManager pool)
    {
        if (poolId == null) return;

        if(poolId == DeflectAudioPoolId)
        {
            _audioPoolManager = pool;
        }
        else if(poolId == HitParticlePoolId)
        {
            _particlePoolManager = pool;
        }
        /*switch (poolId)
        {
            case PoolResourceType.BasicHitParticlePool:
                _particlePoolManager = pool;
                break;
                case PoolResourceType.DeflectAudioPool:
                _audioPoolManager = pool;
                break;
        }*/
        
    }

    private void PlayDeflectionAudio(ProjectileKickType type)
    {
        // Different SFX for different deflection types

        var sfx = _audioPoolManager.GetFromPool(transform.position, transform.rotation) as AudioSource;
        sfx.Play();
        //PoolExtensions.GetAndPlay(_audioPoolManager, transform.position, transform.rotation);
    }
   

    private void SpawnHitParticle(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);
       // ContactPoint contact = collision.contacts[0];
        Vector3 pos = contact.point;
        //Vector3 hitNormal = contact.normal;
      
        var hit = _particlePoolManager.GetFromPool(pos, Quaternion.identity) as ParticleSystem;
        hit.Play();
    
    }

    private void PlayBulletParticle(ProjectileBase bullet/*, BulletType bulletType*/)
    {
        //ParticleSystem particle = transform.root.GetComponentInChildren<ParticleSystem>();
        

        //particleMove.Clear(true); // Clear old particles
        //particleMove.Play(true);  // Force restart from frame 0

        //particleMove.Play();

        if (_particleManager == null) { return; }    

        _particleManager.AddProjectile(bullet/*, bulletType*/);
    }

    private void StopBulletParticle(ProjectileBase bullet/*, BulletType bulletType*/)
    {
        //ParticleSystem particle = transform.root.GetComponentInChildren<ParticleSystem>();
        /*particleMove.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);*/
        
        //particleMove.Pause();

        if (_particleManager == null) { return; }
        _particleManager.RemoveProjectile(bullet);
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
